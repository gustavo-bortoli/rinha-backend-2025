using Payments.Commands;
using Payments.Processors;
using Payments.Summary.Services;
using System.Buffers;
using System.Threading.Channels;

namespace Payments.Workers;

public class PaymentProcessorWorker(
    [FromKeyedServices("worker")] Channel<PaymentCommand> _workerChannel,
    [FromKeyedServices("deadletter")] Channel<PaymentCommand> _deadLetter,
    IServiceScopeFactory _scopeFactory,
    ILogger<PaymentProcessorWorker> _logger) : BackgroundService
{
    private const int MAX_BATCH_SIZE = 40;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batchArray = ArrayPool<PaymentCommand>.Shared.Rent(MAX_BATCH_SIZE);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int count = 0;

                var first = await _workerChannel.Reader.ReadAsync(stoppingToken);
                batchArray[count++] = first;

                while (count < MAX_BATCH_SIZE && _workerChannel.Reader.TryRead(out var msg))
                {
                    batchArray[count++] = msg;
                }

                var tasks = new Task[count];

                for (int i = 0; i < count; i++)
                {
                    var payment = batchArray[i];
                    tasks[i] = ProcessPaymentAsync(payment, stoppingToken);
                }

                await Task.WhenAll(tasks);
            }
        }
        finally
        {
            ArrayPool<PaymentCommand>.Shared.Return(batchArray, true);
        }
    }

    private async Task ProcessPaymentAsync(PaymentCommand payment, CancellationToken ct)
    {
        try
        {
            _logger.ProcessingPayment(payment.CorrelationId);

            using var scope = _scopeFactory.CreateScope();

            var summaryService = scope.ServiceProvider.GetRequiredService<PaymentsSummaryService>();
            var mainProcessor = scope.ServiceProvider.GetRequiredKeyedService<IPaymentProcessor>("main");

            bool processed = false;

            try
            {
                processed = await mainProcessor.ProcessPaymentAsync(payment, ct);
                if (processed)
                {
                    // TODO criar processamento assíncrono para salvar no banco
                    var entity = payment.ToEntity(sentTo: "default");
                    await summaryService.InsertPaymentAsync(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.FailedProcessingPaymentMain(ex, payment.CorrelationId);
            }

            if (!processed)
            {
                var fallbackProcessor = scope.ServiceProvider.GetRequiredKeyedService<IPaymentProcessor>("fallback");

                try
                {
                    processed = await fallbackProcessor.ProcessPaymentAsync(payment, ct);
                    if (processed)
                    {
                        // TODO criar processamento assíncrono para salvar no banco
                        var entity = payment.ToEntity(sentTo: "fallback");
                        await summaryService.InsertPaymentAsync(entity);
                    }
                }
                catch (Exception ex)
                {
                    _logger.FailedProcessingPaymentFallback(ex, payment.CorrelationId);
                }
            }

            if (!processed)
            {
                _logger.NoServiceAvailableSendToDeadLetter(payment.CorrelationId);
                await _deadLetter.Writer.WriteAsync(payment, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.ErrorProcessingPayment(ex, payment.CorrelationId);
            await _deadLetter.Writer.WriteAsync(payment, ct);
        }
    }
}

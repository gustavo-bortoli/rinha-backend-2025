using Payments.Commands;
using System.Buffers;
using System.Threading.Channels;

namespace Payments.Workers;

public class DeadLetterWorker(
    [FromKeyedServices("deadletter")] Channel<PaymentCommand> _deadLetter,
    [FromKeyedServices("worker")] Channel<PaymentCommand> _workerChannel,
    ILogger<DeadLetterWorker> _logger) : BackgroundService
{
    private const int MAX_BATCH_SIZE = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batchArray = ArrayPool<PaymentCommand>.Shared.Rent(MAX_BATCH_SIZE);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int count = 0;

                var first = await _deadLetter.Reader.ReadAsync(stoppingToken);
                batchArray[count++] = first;

                while (count < MAX_BATCH_SIZE && _deadLetter.Reader.TryRead(out var msg))
                {
                    batchArray[count++] = msg;
                }

                var tasks = new Task[count];

                for (int i = 0; i < count; i++)
                {
                    var payment = batchArray[i];
                    tasks[i] = ProcessAsync(payment, stoppingToken);
                }

                await Task.WhenAll(tasks);
            }
        }
        finally
        {
            ArrayPool<PaymentCommand>.Shared.Return(batchArray);
        }
    }

    private async Task ProcessAsync(PaymentCommand payment, CancellationToken stoppingToken)
    {
        try
        {
            _logger.ReprocessingPaymentInDeadLetter(payment.CorrelationId);

            await Task.Delay(500, stoppingToken);

            // Sem regras de número máximo de tentativas por enquanto...
            await _workerChannel.Writer.WriteAsync(payment, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.ErrorRepublishingPaymentFromDeadLetter(ex, payment.CorrelationId);
        }
    }
}

using Payments.Commands;
using System.Text;
using System.Text.Json;

namespace Payments.Processors;

public class FallbackPaymentProcessor(string paymentUrl, HttpClient _httpClient, ILogger<FallbackPaymentProcessor> _logger) : IPaymentProcessor
{
    private static readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromMilliseconds(1500);
    public async Task<bool> ProcessPaymentAsync(PaymentCommand payment, CancellationToken ct)
    {
        _logger.SendingPaymentToFallback(payment.CorrelationId);

        var json = JsonSerializer.Serialize(payment, PaymentCommandJsonContext.Default.PaymentCommand);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        _httpClient.Timeout = REQUEST_TIMEOUT;
        var response = await _httpClient.PostAsync(paymentUrl, content, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.PaymentSentToFallback(payment.CorrelationId);
            return true;
        }
        else
        {
            _logger.PaymentProcessingHttpError(response.StatusCode);
            return false;
        }
    }
}
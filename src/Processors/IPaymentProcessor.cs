using Payments.Commands;

namespace Payments.Processors;
public interface IPaymentProcessor
{
    Task<bool> ProcessPaymentAsync(PaymentCommand payment, CancellationToken ct);
}
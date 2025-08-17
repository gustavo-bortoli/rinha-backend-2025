namespace Payments.DTOs;
public record PaymentRequest(Guid CorrelationId, decimal Amount);
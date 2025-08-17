namespace Payments.DTOs;

public record PaymentSummary(int TotalRequests, decimal TotalAmount);
public record PaymentGroupSummary(PaymentSummary Default, PaymentSummary Fallback);
namespace Payments.Entities;

public record PaymentEntity
{
    // Parameterless default constructor for Dapper Mapper
    public PaymentEntity()
    {

    }
    public PaymentEntity(Guid correlationId, decimal amount, DateTime requestedAt, string sentTo)
    {
        CorrelationId = correlationId;
        Amount = amount;
        RequestedAt = requestedAt;
        SentTo = sentTo;
    }

    public Guid CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
    public string SentTo { get; set; }
};
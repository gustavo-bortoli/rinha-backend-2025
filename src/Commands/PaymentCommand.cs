using Payments.Summary.Entities;
using System.Text.Json.Serialization;

namespace Payments.Commands;
public readonly record struct PaymentCommand(Guid CorrelationId, decimal Amount, DateTime RequestedAt)
{
    public PaymentEntity ToEntity(string sentTo)
        => new(
            correlationId: CorrelationId,
            amount: Amount,
            requestedAt: RequestedAt,
            sentTo: sentTo);
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(PaymentCommand))]
internal partial class PaymentCommandJsonContext : JsonSerializerContext;
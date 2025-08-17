using System.Text.Json.Serialization;

namespace Payments.Commands;
public readonly record struct PaymentCommand(Guid CorrelationId, decimal Amount, DateTime RequestedAt);

[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(PaymentCommand))]
internal partial class PaymentCommandJsonContext : JsonSerializerContext;
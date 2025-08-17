using Dapper;
using Microsoft.AspNetCore.Mvc;
using Payments;
using Payments.Commands;
using Payments.DTOs;
using Payments.Entities;
using Payments.Summary.Services;
using System.Text.Json.Serialization;
using System.Threading.Channels;
[module: DapperAot]

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient();

var (mainPaymentProcessorUrl,
    mainPaymentProcessorStatusPollInterval,
    fallbackPaymentProcessorUrl,
    fallbackPaymentProcessorStatusPollInterval) =
    (
        Environment.GetEnvironmentVariable("PROCESSOR_MAIN_URL"),
        Convert.ToInt32(Environment.GetEnvironmentVariable("PROCESSOR_MAIN_POOL_INTERVAL_STATUS")),
        Environment.GetEnvironmentVariable("PROCESSOR_FALLBACK_URL"),
        Convert.ToInt32(Environment.GetEnvironmentVariable("PROCESSOR_FALLBACK_POOL_INTERVAL_STATUS"))
    );

if (string.IsNullOrWhiteSpace(mainPaymentProcessorUrl))
{
    throw new InvalidOperationException(
        "A variável de ambiente obrigatória 'PROCESSOR_MAIN_URL' não foi definida."
    );
}

if (string.IsNullOrWhiteSpace(fallbackPaymentProcessorUrl))
{
    throw new InvalidOperationException(
        "A variável de ambiente obrigatória 'PROCESSOR_FALLBACK_URL' não foi definida."
    );
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonContext.Default);
    options.SerializerOptions.WriteIndented = false;
});

builder.Services.AddPaymentProcessors(mainPaymentProcessorUrl, fallbackPaymentProcessorUrl);
builder.AddPaymentSummary();

var app = builder.Build();

app.MapPost("/payments", async (
    [FromBody] PaymentRequest request,
    [FromKeyedServices("worker")] Channel<PaymentCommand> ch,
    [FromServices] ILogger<Program> _logger,
    CancellationToken ct) =>
{
    try
    {
        var cmd = new PaymentCommand(
            CorrelationId: request.CorrelationId,
            Amount: request.Amount,
            RequestedAt: DateTime.UtcNow);

        await ch.Writer.WriteAsync(cmd, ct);

        return Results.Accepted();
    }
    catch (Exception ex)
    {
        _logger.FailSchedulingPayment(ex);

        return Results.Problem(
          detail: "Ocorreu um erro ao processar o pagamento no momento",
          statusCode: StatusCodes.Status500InternalServerError
      );
    }
});

app.MapGet("/payments-summary", async (
    [FromQuery] DateTimeOffset? from,
    [FromQuery] DateTimeOffset? to,
    [FromServices] PaymentsSummaryService summaryService) =>
{
    var payments = await summaryService.FindSummaryAsync(from, to);
    return Results.Ok(payments);
});

app.MapPost("/purge-payments", async ([FromServices] PaymentsSummaryService summaryService) =>
{
    await summaryService.PurgeDataAsync();
    return Results.NoContent();
});

app.Run();

[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(PaymentEntity))]
[JsonSerializable(typeof(PaymentRequest))]
[JsonSerializable(typeof(PaymentSummary))]
[JsonSerializable(typeof(PaymentGroupSummary))]
internal partial class AppJsonContext : JsonSerializerContext;
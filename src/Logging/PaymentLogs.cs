using System.Net;

namespace Microsoft.Extensions.Logging;

public static partial class PaymentLogs
{
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Ocorreu um erro ao processar o pagamento no momento")]
    public static partial void FailSchedulingPayment(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Processando pagamento {CorrelationId}")]
    public static partial void ProcessingPayment(this ILogger logger, Guid correlationId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Falha ao processar pagamento {CorrelationId} no Main, tentando fallback...")]
    public static partial void FailedProcessingPaymentMain(this ILogger logger, Exception exception, Guid correlationId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Falha ao processar pagamento {CorrelationId} no Fallback")]
    public static partial void FailedProcessingPaymentFallback(this ILogger logger, Exception exception, Guid correlationId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Nenhum serviço disponível para processar pagamento {CorrelationId}, enviando para DeadLetter")]
    public static partial void NoServiceAvailableSendToDeadLetter(this ILogger logger, Guid correlationId);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Erro ao processar pagamento {CorrelationId}")]
    public static partial void ErrorProcessingPayment(this ILogger logger, Exception exception, Guid correlationId);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Reprocessando pagamento {CorrelationId} na Dead Letter")]
    public static partial void ReprocessingPaymentInDeadLetter(this ILogger logger, Guid correlationId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Error,
        Message = "Erro ao republicar pagamento {CorrelationId} da Dead Letter")]
    public static partial void ErrorRepublishingPaymentFromDeadLetter(this ILogger logger, Exception exception, Guid correlationId);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "Enviando pagamento {CorrelationId} para Main")]
    public static partial void SendingPaymentToMain(this ILogger logger, Guid correlationId);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Information,
        Message = "Pagamento {CorrelationId} enviado com sucesso para Main")]
    public static partial void PaymentSentToMain(this ILogger logger, Guid correlationId);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Warning,
        Message = "Erro ao processar o pagamento. HTTP {StatusCode}")]
    public static partial void PaymentProcessingHttpError(this ILogger logger, HttpStatusCode statusCode);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "Enviando pagamento {CorrelationId} para Fallback")]
    public static partial void SendingPaymentToFallback(this ILogger logger, Guid correlationId);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Information,
        Message = "Pagamento {CorrelationId} enviado com sucesso para Fallback")]
    public static partial void PaymentSentToFallback(this ILogger logger, Guid correlationId);
}


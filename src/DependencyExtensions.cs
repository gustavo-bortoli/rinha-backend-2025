using Payments.Commands;
using Payments.Processors;
using Payments.Summary.Repositories;
using Payments.Summary.Services;
using Payments.Workers;
using System.Threading.Channels;

namespace Payments;

public static class DependencyExtensions
{
    public static WebApplicationBuilder AddPaymentSummary(this WebApplicationBuilder builder)
    {
        var databaseConnString = builder.Configuration.GetConnectionString("PostgressConnection");

        ArgumentException.ThrowIfNullOrEmpty(databaseConnString, "PostgressConnection");

        builder.Services.AddScoped<IPaymentRepository>(sp =>
            new PostgresPaymentRepository(databaseConnString));

        builder.Services.AddScoped<PaymentsSummaryService>();

        return builder;
    }

    public static IServiceCollection AddPaymentProcessors(
        this IServiceCollection services,
        string mainPaymentProcessorUrl,
        string fallbackPaymentProcessorUrl)
    {
        services.AddKeyedSingleton("worker",
            Channel.CreateBounded<PaymentCommand>(new BoundedChannelOptions(100_000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            }));

        services.AddKeyedSingleton("deadletter",
            Channel.CreateUnbounded<PaymentCommand>());

        services.AddHostedService<PaymentProcessorWorker>();
        services.AddHostedService<DeadLetterWorker>();

        services.AddKeyedScoped<IPaymentProcessor>("main", (sp, _) =>
            new MainPaymentProcessor(
                paymentUrl: $"{mainPaymentProcessorUrl}/payments",
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<ILogger<MainPaymentProcessor>>()
            ));

        services.AddKeyedScoped<IPaymentProcessor>("fallback", (sp, _) =>
            new FallbackPaymentProcessor(
                paymentUrl: $"{fallbackPaymentProcessorUrl}/payments",
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<ILogger<FallbackPaymentProcessor>>()
            ));

        return services;
    }
}

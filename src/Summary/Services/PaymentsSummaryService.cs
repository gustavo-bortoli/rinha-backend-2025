using Payments.Commands;
using Payments.DTOs;
using Payments.Entities;
using Payments.Summary.Repositories;

namespace Payments.Summary.Services;

public class PaymentsSummaryService(IPaymentRepository _repository)
{
    public async Task InsertPaymentAsync(PaymentCommand command, string sentTo)
    {
        var entity = new PaymentEntity(
            correlationId: command.CorrelationId,
            amount: command.Amount,
            requestedAt: command.RequestedAt,
            sentTo: sentTo
        );

        await _repository.InsertAsync(entity);
    }

    public async Task<PaymentGroupSummary> FindSummaryAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var payments = await _repository.FindAsync(from, to);

        var summary = payments
            .GroupBy(p => p.SentTo)
            .ToDictionary(
                g => g.Key,
                g => new PaymentSummary(
                    TotalRequests: g.Count(),
                    TotalAmount: g.Sum(p => p.Amount)
                )
            );

        var defaultSummary = summary.GetValueOrDefault("default", new PaymentSummary(0, 0));
        var fallbackSummary = summary.GetValueOrDefault("fallback", new PaymentSummary(0, 0));

        return new PaymentGroupSummary(
            Default: defaultSummary,
            Fallback: fallbackSummary
        );
    }

    public async Task PurgeDataAsync()
        => await _repository.PurgeAsync();
}

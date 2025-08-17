using Payments.Summary.Entities;

namespace Payments.Summary.Repositories;

public interface IPaymentRepository
{
    Task InsertAsync(PaymentEntity payment);
    Task<IEnumerable<PaymentEntity>> FindAsync(DateTimeOffset? from = null, DateTimeOffset? to = null);
    Task PurgeAsync();
}

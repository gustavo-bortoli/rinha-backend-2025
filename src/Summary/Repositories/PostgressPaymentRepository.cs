using Dapper;
using Npgsql;
using Payments.Entities;

namespace Payments.Summary.Repositories;

[DapperAot]
public class PostgresPaymentRepository(string _connectionString) : IPaymentRepository
{
    public async Task InsertAsync(PaymentEntity payment)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql =
            """
            INSERT INTO payments (correlation_id, amount, requested_at, sent_to)
            VALUES (@CorrelationId, @Amount, @RequestedAt, @SentTo)
            ON CONFLICT (correlation_id) DO NOTHING;
            """;

        await connection.ExecuteAsync(sql, payment);
    }

    public async Task<IEnumerable<PaymentEntity>> FindAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql =
            """
            SELECT 
                correlation_id AS CorrelationId, 
                amount AS Amount, 
                requested_at AS RequestedAt, 
                sent_to AS SentTo
            FROM payments
            WHERE (@from IS NULL OR requested_at >= @from)
                AND (@to IS NULL OR requested_at <= @to);
            """;

        return await connection.QueryAsync<PaymentEntity>(sql, new { from, to });
    }

    public async Task PurgeAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync("TRUNCATE TABLE payments", commandTimeout: 30);
    }
}

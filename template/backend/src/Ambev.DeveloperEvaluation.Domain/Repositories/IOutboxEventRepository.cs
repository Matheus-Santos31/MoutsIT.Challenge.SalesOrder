using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IOutboxEventRepository : IBaseRepository<OutboxEvent>
{
    /// <summary>
    /// Pending events that are due for an attempt (never tried, or past their backoff),
    /// oldest first, capped at <paramref name="batchSize"/>.
    /// </summary>
    Task<IEnumerable<OutboxEvent>> GetDueBatchAsync(int batchSize, DateTime now, CancellationToken cancellationToken = default);
}

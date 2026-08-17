using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class OutboxEventRepository : BaseRepository<OutboxEvent>, IOutboxEventRepository
{
    private readonly DefaultContext _context;

    public OutboxEventRepository(DefaultContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OutboxEvent>> GetDueBatchAsync(int batchSize, DateTime now, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .Where(x => x.Status == OutboxEventStatus.Pending && (x.NextAttemptAt == null || x.NextAttemptAt <= now))
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}

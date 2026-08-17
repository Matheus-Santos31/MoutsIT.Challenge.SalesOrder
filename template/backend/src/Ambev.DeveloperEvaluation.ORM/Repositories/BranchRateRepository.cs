using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchRateRepository : BaseRepository<BranchRate>, IBranchRateRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly DefaultContext _context;
    private readonly IDistributedCache _cache;

    public BranchRateRepository(DefaultContext context, IDistributedCache cache) : base(context)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<BranchRate?> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(branchId);

        var cached = await _cache.GetObjectAsync<BranchRate>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var rate = await _context.BranchRates.FirstOrDefaultAsync(x => x.BranchId == branchId, cancellationToken);
        if (rate is not null)
            await _cache.SetObjectAsync(cacheKey, rate, CacheDuration, cancellationToken);

        return rate;
    }

    public override async Task UpdateAsync(BranchRate entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.BranchId), cancellationToken);
    }

    public override async Task DeleteAsync(BranchRate entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.BranchId), cancellationToken);
    }

    private static string BuildCacheKey(Guid branchId) => $"branch-rate:{branchId}";
}

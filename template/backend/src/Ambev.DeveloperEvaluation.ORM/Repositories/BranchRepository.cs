using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class BranchRepository : BaseRepository<Branch>, IBranchRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly DefaultContext _context;
    private readonly IDistributedCache _cache;

    public BranchRepository(DefaultContext context, IDistributedCache cache) : base(context)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Branch?> GetByDocNumberAsync(string docNumber, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(x => x.DocNumber == docNumber && (excludeId == null || x.Id != excludeId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public override async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(id);

        var cached = await _cache.GetObjectAsync<Branch>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var branch = await base.GetByIdAsync(id, cancellationToken);
        if (branch is not null)
            await _cache.SetObjectAsync(cacheKey, branch, CacheDuration, cancellationToken);

        return branch;
    }

    public override async Task UpdateAsync(Branch entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.Id), cancellationToken);
    }

    public override async Task DeleteAsync(Branch entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.Id), cancellationToken);
    }

    private static string BuildCacheKey(Guid id) => $"branch:{id}";
}

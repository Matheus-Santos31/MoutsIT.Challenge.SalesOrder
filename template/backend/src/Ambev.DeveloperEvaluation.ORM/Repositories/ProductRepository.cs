using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Cache-aside on GetByIdAsync: check Redis, miss falls through to Postgres and populates
/// the cache; Update/Delete invalidate the key immediately. A 5-minute TTL is a safety net
/// in case any write path ever bypasses this repository and the key doesn't get invalidated.
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IDistributedCache _cache;

    public ProductRepository(DefaultContext context, IDistributedCache cache) : base(context)
    {
        _cache = cache;
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(id);

        var cached = await _cache.GetObjectAsync<Product>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var product = await base.GetByIdAsync(id, cancellationToken);
        if (product is not null)
            await _cache.SetObjectAsync(cacheKey, product, CacheDuration, cancellationToken);

        return product;
    }

    public override async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.Id), cancellationToken);
    }

    public override async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.Id), cancellationToken);
    }

    private static string BuildCacheKey(Guid id) => $"product:{id}";
}

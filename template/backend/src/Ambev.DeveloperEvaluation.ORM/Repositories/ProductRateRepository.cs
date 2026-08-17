using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class ProductRateRepository : BaseRepository<ProductRate>, IProductRateRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly DefaultContext _context;
    private readonly IDistributedCache _cache;

    public ProductRateRepository(DefaultContext context, IDistributedCache cache) : base(context)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ProductRate?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(productId);

        var cached = await _cache.GetObjectAsync<ProductRate>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var rate = await _context.ProductRates.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
        if (rate is not null)
            await _cache.SetObjectAsync(cacheKey, rate, CacheDuration, cancellationToken);

        return rate;
    }

    public override async Task UpdateAsync(ProductRate entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.ProductId), cancellationToken);
    }

    public override async Task DeleteAsync(ProductRate entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken);
        await _cache.RemoveAsync(BuildCacheKey(entity.ProductId), cancellationToken);
    }

    private static string BuildCacheKey(Guid productId) => $"product-rate:{productId}";
}

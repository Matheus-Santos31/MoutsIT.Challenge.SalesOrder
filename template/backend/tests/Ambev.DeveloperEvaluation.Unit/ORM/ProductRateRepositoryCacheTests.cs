using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

/// <summary>
/// cache aside behavior tests
/// </summary>
public class ProductRateRepositoryCacheTests
{
    private static DefaultContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DefaultContext(options);
    }

    [Fact(DisplayName = "Given a cached rate When GetByProductIdAsync is called Then returns the cached value without querying the database")]
    public async Task GetByProductIdAsync_CacheHit_ReturnsCachedValueWithoutQueryingDatabase()
    {
        await using var context = BuildContext();
        var productId = Guid.NewGuid();

        context.ProductRates.Add(new ProductRate { ProductId = productId, AverageRate = 1m, ReviewCount = 1 });
        await context.SaveChangesAsync();

        var cachedRate = new ProductRate { ProductId = productId, AverageRate = 4.5m, ReviewCount = 10 };
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"product-rate:{productId}", Arg.Any<CancellationToken>())
            .Returns(JsonSerializer.SerializeToUtf8Bytes(cachedRate));

        var repository = new ProductRateRepository(context, cache);

        var result = await repository.GetByProductIdAsync(productId);

        result.Should().NotBeNull();
        result!.ReviewCount.Should().Be(10);
    }

    [Fact(DisplayName = "Given no cached rate When GetByProductIdAsync is called Then reads the database and populates the cache")]
    public async Task GetByProductIdAsync_CacheMiss_ReadsDatabaseAndPopulatesCache()
    {
        await using var context = BuildContext();
        var productId = Guid.NewGuid();
        context.ProductRates.Add(new ProductRate { ProductId = productId, AverageRate = 4.5m, ReviewCount = 10 });
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"product-rate:{productId}", Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        var repository = new ProductRateRepository(context, cache);

        var result = await repository.GetByProductIdAsync(productId);

        result.Should().NotBeNull();
        result!.ReviewCount.Should().Be(10);
        await cache.Received(1).SetAsync(
            $"product-rate:{productId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "UpdateAsync invalidates the rate's cache entry")]
    public async Task UpdateAsync_InvalidatesCacheEntry()
    {
        await using var context = BuildContext();
        var rate = new ProductRate { ProductId = Guid.NewGuid(), AverageRate = 4.5m, ReviewCount = 10 };
        context.ProductRates.Add(rate);
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        var repository = new ProductRateRepository(context, cache);

        await repository.UpdateAsync(rate);

        await cache.Received(1).RemoveAsync($"product-rate:{rate.ProductId}", Arg.Any<CancellationToken>());
    }
}

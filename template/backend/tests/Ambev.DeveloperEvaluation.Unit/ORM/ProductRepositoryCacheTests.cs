using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

/// <summary>
/// cache-aside behavior tests
/// </summary>
public class ProductRepositoryCacheTests
{
    private static DefaultContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DefaultContext(options);
    }

    [Fact(DisplayName = "Given a cached product When GetByIdAsync is called Then returns the cached value without querying the database")]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedValueWithoutQueryingDatabase()
    {
        await using var context = BuildContext();
        var productId = Guid.NewGuid();

        // A different product sits in the "database" — proves the cached value wins.
        context.Products.Add(new Product { Id = productId, Title = "From DB", Price = 1m, Category = ProductCategory.Food });
        await context.SaveChangesAsync();

        var cachedProduct = new Product { Id = productId, Title = "From Cache", Price = 99m, Category = ProductCategory.Food };
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"product:{productId}", Arg.Any<CancellationToken>())
            .Returns(JsonSerializer.SerializeToUtf8Bytes(cachedProduct));

        var repository = new ProductRepository(context, cache);

        var result = await repository.GetByIdAsync(productId);

        result.Should().NotBeNull();
        result!.Title.Should().Be("From Cache");
    }

    [Fact(DisplayName = "Given no cached product When GetByIdAsync is called Then reads the database and populates the cache")]
    public async Task GetByIdAsync_CacheMiss_ReadsDatabaseAndPopulatesCache()
    {
        await using var context = BuildContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product { Id = productId, Title = "From DB", Price = 1m, Category = ProductCategory.Food });
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"product:{productId}", Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        var repository = new ProductRepository(context, cache);

        var result = await repository.GetByIdAsync(productId);

        result.Should().NotBeNull();
        result!.Title.Should().Be("From DB");
        await cache.Received(1).SetAsync(
            $"product:{productId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "UpdateAsync invalidates the product's cache entry")]
    public async Task UpdateAsync_InvalidatesCacheEntry()
    {
        await using var context = BuildContext();
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer", Price = 10m, Category = ProductCategory.Food };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        var repository = new ProductRepository(context, cache);

        await repository.UpdateAsync(product);

        await cache.Received(1).RemoveAsync($"product:{product.Id}", Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "DeleteAsync invalidates the product's cache entry")]
    public async Task DeleteAsync_InvalidatesCacheEntry()
    {
        await using var context = BuildContext();
        var product = new Product { Id = Guid.NewGuid(), Title = "Beer", Price = 10m, Category = ProductCategory.Food };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        var repository = new ProductRepository(context, cache);

        await repository.DeleteAsync(product);

        await cache.Received(1).RemoveAsync($"product:{product.Id}", Arg.Any<CancellationToken>());
    }
}

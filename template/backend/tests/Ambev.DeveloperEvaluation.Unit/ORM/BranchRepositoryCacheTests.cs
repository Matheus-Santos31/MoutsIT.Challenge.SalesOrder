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
/// cache-aside behavior tests
/// </summary>
public class BranchRepositoryCacheTests
{
    private static DefaultContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DefaultContext(options);
    }

    [Fact(DisplayName = "Given a cached branch When GetByIdAsync is called Then returns the cached value without querying the database")]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedValueWithoutQueryingDatabase()
    {
        await using var context = BuildContext();
        var branchId = Guid.NewGuid();

        context.Branches.Add(new Branch { Id = branchId, Name = "From DB", DocNumber = "1", CompanyName = "Co" });
        await context.SaveChangesAsync();

        var cachedBranch = new Branch { Id = branchId, Name = "From Cache", DocNumber = "1", CompanyName = "Co" };
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"branch:{branchId}", Arg.Any<CancellationToken>())
            .Returns(JsonSerializer.SerializeToUtf8Bytes(cachedBranch));

        var repository = new BranchRepository(context, cache);

        var result = await repository.GetByIdAsync(branchId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("From Cache");
    }

    [Fact(DisplayName = "Given no cached branch When GetByIdAsync is called Then reads the database and populates the cache")]
    public async Task GetByIdAsync_CacheMiss_ReadsDatabaseAndPopulatesCache()
    {
        await using var context = BuildContext();
        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "From DB", DocNumber = "1", CompanyName = "Co" });
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync($"branch:{branchId}", Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        var repository = new BranchRepository(context, cache);

        var result = await repository.GetByIdAsync(branchId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("From DB");
        await cache.Received(1).SetAsync(
            $"branch:{branchId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "UpdateAsync invalidates the branch's cache entry")]
    public async Task UpdateAsync_InvalidatesCacheEntry()
    {
        await using var context = BuildContext();
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Downtown", DocNumber = "1", CompanyName = "Co" };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        var repository = new BranchRepository(context, cache);

        await repository.UpdateAsync(branch);

        await cache.Received(1).RemoveAsync($"branch:{branch.Id}", Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "DeleteAsync invalidates the branch's cache entry")]
    public async Task DeleteAsync_InvalidatesCacheEntry()
    {
        await using var context = BuildContext();
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Downtown", DocNumber = "1", CompanyName = "Co" };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var cache = Substitute.For<IDistributedCache>();
        var repository = new BranchRepository(context, cache);

        await repository.DeleteAsync(branch);

        await cache.Received(1).RemoveAsync($"branch:{branch.Id}", Arg.Any<CancellationToken>());
    }
}

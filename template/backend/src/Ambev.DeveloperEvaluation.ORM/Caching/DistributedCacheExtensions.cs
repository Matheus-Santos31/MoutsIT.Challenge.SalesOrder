using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace Ambev.DeveloperEvaluation.ORM.Caching;

/// <summary>
/// Cache Extension Cache Aside Approach.
/// </summary>
public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public static async Task<T?> GetObjectAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes is null || bytes.Length == 0)
            return default;

        return JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
    }

    public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        await cache.SetAsync(key, bytes, options, cancellationToken);
    }
}

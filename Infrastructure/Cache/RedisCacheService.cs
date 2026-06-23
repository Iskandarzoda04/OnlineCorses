using System.Text.Json;
using Application.Interfaces.Services;
using StackExchange.Redis;

namespace Infrastructure.Cache;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(value);
        return _db.StringSetAsync(key, json, ttl);
    }

    public Task RemoveAsync(string key) => _db.KeyDeleteAsync(key);

}

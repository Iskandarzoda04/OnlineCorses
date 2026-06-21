using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Services;
using StackExchange.Redis;

namespace Infrastructure.Cache;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService, IRedisCacheService
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

    public Task SetData<T>(string key, T value, int expirationMinutes)
    {
        var json = JsonSerializer.Serialize(value);
        return _db.StringSetAsync(key, json, TimeSpan.FromMinutes(expirationMinutes));
    }

    public async Task<T?> GetData<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public Task RemoveData(string key) => _db.KeyDeleteAsync(key);
}

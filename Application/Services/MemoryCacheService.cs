using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class MemoryCacheService(IMemoryCache memoryCache,ILogger<MemoryCacheService> logger) : IMemoryCacheService
{
    public async Task SetData<T>(string key, T data, int expirationMinutes)
    {
       var cacheOptions = new MemoryCacheEntryOptions()

       {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes), 
            SlidingExpiration = TimeSpan.FromMinutes(expirationMinutes)
       };

         memoryCache.Set(key, data, cacheOptions);
        await Console.Out.WriteLineAsync(new string('*', 120));
        logger.LogInformation("Memory cache : Add new data to cache by key : {key}, expiration time before : {exprirationTime}", key, expirationMinutes);
        await Console.Out.WriteLineAsync(new string('*', 120));
    }

    public async Task<T?> GetData<T>(string key)
    {
         var data = memoryCache.Get<T>(key);
        
        await Console.Out.WriteLineAsync(new string('*', 120));
        logger.LogInformation("Memory cache : Data retrieved from cache  key : {key}", key);
        await Console.Out.WriteLineAsync(new string('*', 120));
        
        return data;
    }

    public async Task DeleteData(string key)
    {
         memoryCache.Remove(key);
        
        await Console.Out.WriteLineAsync(new string('*', 120));
        logger.LogInformation("Memory cache : Deleted data in cache by key : {key}", key);
        await Console.Out.WriteLineAsync(new string('*', 120));
    }
}

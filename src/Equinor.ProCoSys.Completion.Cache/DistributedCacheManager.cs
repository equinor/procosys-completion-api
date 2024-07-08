using System.Text.Json;
using Equinor.ProCoSys.Common.Caches;
using Microsoft.Extensions.Caching.Distributed;

namespace Equinor.ProCoSys.Completion.Cache;

public sealed class DistributedCacheManager(IDistributedCache distributedCache) : ICacheManager
{
    public T Get<T>(string key) where T : class
    {
        var entryType = typeof(T);
        var item = GetObjectFromCache(entryType, key);
        return IsTask(entryType)
            ? CreateTask<T>(item)
            : (T) item;
    }

    public void Remove(string key) => distributedCache.Remove(key);

    public T GetOrCreate<T>(string key, Func<T> fetch, CacheDuration duration, long expiration) where T : class
    {
        var entryType = typeof(T);

        var item = GetObjectFromCache(entryType, key);
        if (item is not null)
        {
            return IsTask(entryType)
                ? CreateTask<T>(item)
                : (T)item;
        }

        var fetched = FetchObject(fetch);
        var serialized = JsonSerializer.Serialize(fetched);
        distributedCache.SetString(key, serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = GetEntryCacheDuration(duration, expiration)
            });

        return IsTask(entryType)
            ? CreateTask<T>(fetched)
            : (T)fetched!;
    }

    private object? GetObjectFromCache(Type entryType, string key)
    {
        var entry = distributedCache.GetString(key);
        var deserialized = JsonSerializer.Deserialize(entry ?? "null", TaskUnwrapper(entryType));

        return deserialized;
    }
    
    private static T CreateTask<T>(object? value)
    {
        var innerType = typeof(T).GetGenericArguments()[0];
        var taskFromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(innerType);
        return (T)taskFromResultMethod.Invoke(null, [value])!;
    }

    private static object? FetchObject<T>(Func<T> fetch)
    {
        var entryType = typeof(T);

        object? value = fetch();
        if (!IsTask(entryType))
        {
            return value;
        }

        var task = fetch() as Task;
        task!.Wait();
            
        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty!.GetValue(task);
        value = result;

        return value;
    }

    private static bool IsTask(Type t)
        => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>);

    private static Type TaskUnwrapper(Type t) => IsTask(t) ? t.GetGenericArguments()[0] : t;


    private static TimeSpan GetEntryCacheDuration(CacheDuration duration, long expiration) =>
        duration switch
        {
            CacheDuration.Seconds => TimeSpan.FromSeconds(expiration),
            CacheDuration.Minutes => TimeSpan.FromMinutes(expiration),
            CacheDuration.Hours => TimeSpan.FromHours(expiration),
            _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
        };
}

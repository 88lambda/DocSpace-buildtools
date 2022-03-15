namespace ASC.Common.Caching;

[Singletone]
public class AscCacheNotify
{
    private readonly ICacheNotify<AscCacheItem> _cacheNotify;

    public AscCacheNotify(ICacheNotify<AscCacheItem> cacheNotify)
    {
        _cacheNotify = cacheNotify;

        _cacheNotify.Subscribe((item) => { OnClearCache(); }, CacheNotifyAction.Any);
    }

    public void ClearCache() => _cacheNotify.Publish(new AscCacheItem { Id = Guid.NewGuid().ToString() }, CacheNotifyAction.Any);

    public static void OnClearCache()
    {
            var keys = MemoryCache.Default.Select(r => r.Key);

        foreach (var k in keys)
        {
            MemoryCache.Default.Remove(k);
        }
    }
}

[Singletone]
public class AscCache : ICache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, object> _memoryCacheKeys;

    public AscCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _memoryCacheKeys = new ConcurrentDictionary<string, object>();
    }

    public T Get<T>(string key) where T : class
    {
        return _memoryCache.Get<T>(key);
    }

    public void Insert(string key, object value, TimeSpan sligingExpiration)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(sligingExpiration)
            .RegisterPostEvictionCallback(EvictionCallback);

        _memoryCache.Set(key, value, options);
        _memoryCacheKeys.TryAdd(key, null);
    }

    public void Insert(string key, object value, DateTime absolutExpiration)
    {
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(absolutExpiration == DateTime.MaxValue ? DateTimeOffset.MaxValue : new DateTimeOffset(absolutExpiration))
            .RegisterPostEvictionCallback(EvictionCallback);

        _memoryCache.Set(key, value, options);
        _memoryCacheKeys.TryAdd(key, null);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public void Remove(Regex pattern)
    {
        var copy = _memoryCacheKeys.ToDictionary(p => p.Key, p => p.Value);
            var keys = copy.Select(p => p.Key).Where(k => pattern.IsMatch(k));

        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
        }
    }

    public ConcurrentDictionary<string, T> HashGetAll<T>(string key) =>
        _memoryCache.GetOrCreate(key, r => new ConcurrentDictionary<string, T>());

    public T HashGet<T>(string key, string field)
    {
        if (_memoryCache.TryGetValue<ConcurrentDictionary<string, T>>(key, out var dic)
            && dic.TryGetValue(field, out var value))
        {
            return value;
        }

        return default;
    }

    public void HashSet<T>(string key, string field, T value)
    {
        var dic = HashGetAll<T>(key);
        if (value != null)
        {
            dic.AddOrUpdate(field, value, (k, v) => value);
            _memoryCache.Set(key, dic, DateTime.MaxValue);
        }
        else if (dic != null)
        {
            dic.TryRemove(field, out _);

            if (dic.IsEmpty)
            {
                _memoryCache.Remove(key);
            }
            else
            {
                _memoryCache.Set(key, dic, DateTime.MaxValue);
            }
        }
    }

    private void EvictionCallback(object key, object value, EvictionReason reason, object state)
    {
        _memoryCacheKeys.TryRemove(key.ToString(), out _);
    }
}
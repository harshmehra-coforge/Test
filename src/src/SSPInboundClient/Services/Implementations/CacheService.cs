using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using SSPInboundClient.Services.Interfaces;
using SSPInboundClient.Services.Models;
using System.Text.Json;

namespace SSPInboundClient.Services.Implementations
{
    /// <summary>
    /// Cache service implementation with multi-level caching support
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly IConfiguration _configuration;

        public CacheService(
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            ILogger<CacheService> logger,
            IConfiguration configuration)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<T?> GetAsync<T>(string key, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default)
        {
            try
            {
                switch (level)
                {
                    case CacheLevel.Memory:
                        return GetFromMemoryCache<T>(key);
                        
                    case CacheLevel.Distributed:
                        return await GetFromDistributedCacheAsync<T>(key, cancellationToken);
                        
                    case CacheLevel.Both:
                        // Try memory cache first
                        var memoryValue = GetFromMemoryCache<T>(key);
                        if (memoryValue != null)
                        {
                            _logger.LogDebug("Cache hit (Memory): {Key}", key);
                            return memoryValue;
                        }
                        
                        // Fall back to distributed cache
                        var distributedValue = await GetFromDistributedCacheAsync<T>(key, cancellationToken);
                        if (distributedValue != null)
                        {
                            _logger.LogDebug("Cache hit (Distributed): {Key}", key);
                            // Store in memory cache for faster access
                            var memoryExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:MemoryExpirationMinutes", 5));
                            SetInMemoryCache(key, distributedValue, memoryExpiration);
                        }
                        
                        return distributedValue;
                        
                    default:
                        throw new ArgumentException($"Invalid cache level: {level}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache. Key: {Key}, Level: {Level}", key, level);
                return default(T);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default)
        {
            try
            {
                var defaultExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:DefaultExpirationMinutes", 60));
                var expirationTime = expiration ?? defaultExpiration;
                
                switch (level)
                {
                    case CacheLevel.Memory:
                        SetInMemoryCache(key, value, expirationTime);
                        break;
                        
                    case CacheLevel.Distributed:
                        await SetInDistributedCacheAsync(key, value, expirationTime, cancellationToken);
                        break;
                        
                    case CacheLevel.Both:
                        var memoryExpiration = TimeSpan.FromMinutes(Math.Min(5, (int)expirationTime.TotalMinutes));
                        SetInMemoryCache(key, value, memoryExpiration);
                        await SetInDistributedCacheAsync(key, value, expirationTime, cancellationToken);
                        break;
                }
                
                _logger.LogDebug("Cache set: {Key}, Level: {Level}, Expiration: {Expiration}", key, level, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache. Key: {Key}, Level: {Level}", key, level);
            }
        }

        public async Task RemoveAsync(string key, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default)
        {
            try
            {
                switch (level)
                {
                    case CacheLevel.Memory:
                        _memoryCache.Remove(key);
                        break;
                        
                    case CacheLevel.Distributed:
                        await _distributedCache.RemoveAsync(key, cancellationToken);
                        break;
                        
                    case CacheLevel.Both:
                        _memoryCache.Remove(key);
                        await _distributedCache.RemoveAsync(key, cancellationToken);
                        break;
                }
                
                _logger.LogDebug("Cache removed: {Key}, Level: {Level}", key, level);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cache. Key: {Key}, Level: {Level}", key, level);
            }
        }

        public async Task<bool> ExistsAsync(string key, CacheLevel level = CacheLevel.Distributed, CancellationToken cancellationToken = default)
        {
            try
            {
                switch (level)
                {
                    case CacheLevel.Memory:
                        return _memoryCache.TryGetValue(key, out _);
                        
                    case CacheLevel.Distributed:
                        var value = await _distributedCache.GetAsync(key, cancellationToken);
                        return value != null;
                        
                    case CacheLevel.Both:
                        if (_memoryCache.TryGetValue(key, out _))
                            return true;
                        
                        var distributedValue = await _distributedCache.GetAsync(key, cancellationToken);
                        return distributedValue != null;
                        
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence. Key: {Key}, Level: {Level}", key, level);
                return false;
            }
        }

        private T? GetFromMemoryCache<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default(T);
        }

        private async Task<T?> GetFromDistributedCacheAsync<T>(string key, CancellationToken cancellationToken)
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (cachedValue != null)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(cachedValue);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached value for key: {Key}", key);
                    // Remove corrupted cache entry
                    await _distributedCache.RemoveAsync(key, cancellationToken);
                }
            }
            return default(T);
        }

        private void SetInMemoryCache<T>(string key, T value, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(Math.Min(5, (int)expiration.TotalMinutes / 2)),
                Priority = CacheItemPriority.Normal
            };
            
            _memoryCache.Set(key, value, options);
        }

        private async Task SetInDistributedCacheAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration,
                    SlidingExpiration = TimeSpan.FromMinutes(Math.Min(30, (int)expiration.TotalMinutes / 2))
                };
                
                await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to serialize value for caching. Key: {Key}", key);
                throw;
            }
        }
    }
}
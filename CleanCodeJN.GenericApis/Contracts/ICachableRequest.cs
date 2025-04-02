namespace CleanCodeJN.GenericApis.Contracts;

/// <summary>
/// Add this to a request object to cache the response
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// Bypass the cache
    /// </summary>
    bool BypassCache { get; }

    /// <summary>
    /// The cache key
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// The cache duration
    /// </summary>
    TimeSpan? CacheDuration { get; }
}

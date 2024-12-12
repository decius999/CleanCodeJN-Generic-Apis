namespace CleanCodeJN.GenericApis.Contracts;

public interface ICacheableRequest
{
    bool BypassCache { get; }
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}

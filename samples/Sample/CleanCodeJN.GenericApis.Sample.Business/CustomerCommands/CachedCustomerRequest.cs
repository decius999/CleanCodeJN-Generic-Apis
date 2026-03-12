using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Represents a request to retrieve a list of customers, with support for caching.
/// </summary>
/// <remarks>This request is cacheable and includes properties to control cache behavior, such as bypassing the
/// cache, specifying a cache key, and defining the cache duration.</remarks>
public class CachedCustomerRequest : IRequest<BaseListResponse<Customer>>, ICacheableRequest
{
    public bool BypassCache { get; }

    public string CacheKey => "customer";

    public TimeSpan? CacheDuration => TimeSpan.FromDays(1);
}

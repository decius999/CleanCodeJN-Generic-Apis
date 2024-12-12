using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;
public class CachedCustomerRequest : IRequest<BaseListResponse<Customer>>, ICacheableRequest
{
    public bool BypassCache { get; }

    public string CacheKey => "customer";

    public TimeSpan? CacheDuration => TimeSpan.FromDays(1);
}

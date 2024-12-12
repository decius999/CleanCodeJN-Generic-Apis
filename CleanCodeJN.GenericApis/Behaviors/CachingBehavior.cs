using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace CleanCodeJN.GenericApis.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response;
        if (request.BypassCache)
        {
            return await next();
        }

        async Task<TResponse> GetResponseAndAddToCache()
        {
            response = await next();

            var slidingExpiration = request.CacheDuration;

            var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
            var serializedData = Encoding.Default.GetBytes(JsonSerializer.Serialize(response,
                new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }));

            await cache.SetAsync(request.CacheKey, serializedData, options, cancellationToken);

            return response;
        }

        var cachedResponse = await cache.GetAsync(request.CacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse));
            logger.LogDebug($"Fetched from Cache -> '{request.CacheKey}'.");
        }
        else
        {
            response = await GetResponseAndAddToCache();
            logger.LogDebug($"Added to Cache -> '{request.CacheKey}'.");
        }

        return response;
    }
}

using System.Text;
using System.Text.Json;
using CleanCodeJN.GenericApis.Behaviors;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Behaviors;

/// <summary>
/// Contains unit tests for <see cref="CachingBehavior{TRequest, TResponse}"/>.
/// </summary>
public class CachingBehaviorTests
{
    /// <summary>
    /// Verifies that a cached response is returned without calling the next handler on a cache hit.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCachedResponse_WhenCacheHit()
    {
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<CachingBehavior<TestCacheableRequest, string>>>();
        var expectedResponse = "cached value";
        var serialized = Encoding.Default.GetBytes(JsonSerializer.Serialize(expectedResponse));
        mockCache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serialized);

        var behavior = new CachingBehavior<TestCacheableRequest, string>(mockCache.Object, mockLogger.Object);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ => { nextCalled = true; return Task.FromResult("fresh value"); };

        var result = await behavior.Handle(new TestCacheableRequest(), next, CancellationToken.None);

        Assert.Equal(expectedResponse, result);
        Assert.False(nextCalled, "next should not be called on cache hit");
    }

    /// <summary>
    /// Verifies that the next handler is called and the result is stored in the cache on a cache miss.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCallNextAndStoreInCache_WhenCacheMiss()
    {
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<CachingBehavior<TestCacheableRequest, string>>>();
        var freshResponse = "fresh value";
        mockCache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null);

        var behavior = new CachingBehavior<TestCacheableRequest, string>(mockCache.Object, mockLogger.Object);
        var request = new TestCacheableRequest();
        RequestHandlerDelegate<string> next = _ => Task.FromResult(freshResponse);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        Assert.Equal(freshResponse, result);
        mockCache.Verify(c => c.SetAsync(
            request.CacheKey,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the cache is completely bypassed when <c>BypassCache</c> is true on the request.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldBypassCache_WhenBypassCacheIsTrue()
    {
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<CachingBehavior<TestBypassCacheRequest, string>>>();
        var freshResponse = "bypass value";

        var behavior = new CachingBehavior<TestBypassCacheRequest, string>(mockCache.Object, mockLogger.Object);
        RequestHandlerDelegate<string> next = _ => Task.FromResult(freshResponse);

        var result = await behavior.Handle(new TestBypassCacheRequest(), next, CancellationToken.None);

        Assert.Equal(freshResponse, result);
        mockCache.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the cache is queried using the exact cache key provided by the request.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUseCacheKey_FromRequest()
    {
        var mockCache = new Mock<IDistributedCache>();
        var mockLogger = new Mock<ILogger<CachingBehavior<TestCacheableRequest, string>>>();
        mockCache
            .Setup(c => c.GetAsync("my-cache-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null);

        var behavior = new CachingBehavior<TestCacheableRequest, string>(mockCache.Object, mockLogger.Object);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("value");

        await behavior.Handle(new TestCacheableRequest(), next, CancellationToken.None);

        mockCache.Verify(c => c.GetAsync("my-cache-key", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// A cacheable test request that uses a fixed cache key and does not bypass the cache.
    /// </summary>
    public class TestCacheableRequest : IRequest<string>, ICacheableRequest
    {
        /// <summary>
        /// Gets a value indicating whether the cache should be bypassed; always <c>false</c>.
        /// </summary>
        public bool BypassCache => false;

        /// <summary>
        /// Gets the cache key used for storing and retrieving the response.
        /// </summary>
        public string CacheKey => "my-cache-key";

        /// <summary>
        /// Gets the duration for which the cached response is considered valid.
        /// </summary>
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }

    /// <summary>
    /// A cacheable test request that is configured to always bypass the cache.
    /// </summary>
    public class TestBypassCacheRequest : IRequest<string>, ICacheableRequest
    {
        /// <summary>
        /// Gets a value indicating whether the cache should be bypassed; always <c>true</c>.
        /// </summary>
        public bool BypassCache => true;

        /// <summary>
        /// Gets the cache key that would be used if caching were not bypassed.
        /// </summary>
        public string CacheKey => "bypass-key";

        /// <summary>
        /// Gets the duration for which a cached response would be valid.
        /// </summary>
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}

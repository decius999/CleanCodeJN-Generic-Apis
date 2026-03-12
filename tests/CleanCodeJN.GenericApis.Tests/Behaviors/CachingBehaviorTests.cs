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

public class CachingBehaviorTests
{
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

    public class TestCacheableRequest : IRequest<string>, ICacheableRequest
    {
        public bool BypassCache => false;
        public string CacheKey => "my-cache-key";
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }

    public class TestBypassCacheRequest : IRequest<string>, ICacheableRequest
    {
        public bool BypassCache => true;
        public string CacheKey => "bypass-key";
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}

using CleanCodeJN.GenericApis.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Behaviors;

public class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_AndReturnResponse()
    {
        var mockLogger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(mockLogger.Object);
        var expectedResponse = "test response";
        RequestHandlerDelegate<string> next = _ => Task.FromResult(expectedResponse);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_ForNonGenericRequest()
    {
        var mockLogger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(mockLogger.Object);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("result");

        await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WithGenericTypeArguments_ForGenericRequest()
    {
        var mockLogger = new Mock<ILogger<LoggingBehavior<TestGenericRequest<int>, string>>>();
        var behavior = new LoggingBehavior<TestGenericRequest<int>, string>(mockLogger.Object);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("result");

        await behavior.Handle(new TestGenericRequest<int>(), next, CancellationToken.None);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Int32")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldStillReturnResponse_WhenNextThrows()
    {
        var mockLogger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(mockLogger.Object);
        RequestHandlerDelegate<string> next = _ => throw new InvalidOperationException("boom");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new TestRequest(), next, CancellationToken.None));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    public class TestRequest : IRequest<string> { }
    public class TestGenericRequest<T> : IRequest<string> { }
}

using CleanCodeJN.GenericApis.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Behaviors;

/// <summary>
/// Contains unit tests for <see cref="LoggingBehavior{TRequest, TResponse}"/>.
/// </summary>
public class LoggingBehaviorTests
{
    /// <summary>
    /// Verifies that the behavior calls the next handler and returns its response.
    /// </summary>
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

    /// <summary>
    /// Verifies that the behavior logs an information message containing the request type name.
    /// </summary>
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

    /// <summary>
    /// Verifies that the behavior includes generic type argument names in the log message for generic requests.
    /// </summary>
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

    /// <summary>
    /// Verifies that the behavior logs before the exception propagates when the next handler throws.
    /// </summary>
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

    /// <summary>
    /// A non-generic test request used in logging behavior tests.
    /// </summary>
    public class TestRequest : IRequest<string> { }

    /// <summary>
    /// A generic test request used to verify that type arguments appear in log output.
    /// </summary>
    public class TestGenericRequest<T> : IRequest<string> { }
}

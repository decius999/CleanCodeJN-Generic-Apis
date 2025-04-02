using System.Diagnostics;
using MediatR;

namespace CleanCodeJN.GenericApis.Behaviors;

/// <summary>
/// The logging behavior for logging the request time.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="logger">The logger.</param>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// The Logging Behavior for Mediatr
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>TResponse</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response;
        var requestType = request.GetType();
        var requestName = requestType.Name.Split("`")[0];

        if (requestType.IsGenericType)
        {
            var genericType = requestType.GetGenericTypeDefinition();
            var genericArguments = requestType.GetGenericArguments();
            requestName += $"<{string.Join(", ", genericArguments.Select(arg => arg.Name))}>";
        }

        Stopwatch stopwatch = new();

        try
        {
            stopwatch.Start();
            response = await next();
            stopwatch.Stop();
        }
        finally
        {
            logger.LogInformation($"{requestName} in {stopwatch.ElapsedMilliseconds}ms");
        }

        return response;
    }
}

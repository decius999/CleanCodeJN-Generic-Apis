using System.Diagnostics;
using MediatR;

namespace CleanCodeJN.GenericApis.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
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

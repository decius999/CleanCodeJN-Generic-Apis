using System.Diagnostics;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Behaviors;

/// <summary>
/// MediatR pipeline behavior that short-circuits request execution when a debugger is attached, logging the request type instead of forwarding to the next handler.
/// </summary>
/// <typeparam name="TRequest">The request type that implements <see cref="IPreventExecutionOnLocalEnvironment"/>.</typeparam>
/// <typeparam name="TResponse">The response type derived from <see cref="Response"/>.</typeparam>
public class LocalEnvironmentBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPreventExecutionOnLocalEnvironment
    where TResponse : Response
{
    /// <summary>
    /// Intercepts the request pipeline and returns a successful response without executing the real handler when a debugger is attached.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token to observe for cancellation of the operation.</param>
    /// <returns>A successful <see cref="Response"/> when debugging locally; otherwise the result of the next handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (Debugger.IsAttached)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{request.GetType().FullName} executed!");
            Console.ResetColor();

            return (TResponse)new Response(ResultEnum.SUCCESS);
        }

        return await next();
    }
}

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Behaviors;

/// <summary>
/// MediatR pipeline behavior that short-circuits execution when a debugger is attached, printing the serialized event payload to the console instead of forwarding to the real handler.
/// </summary>
/// <typeparam name="TRequest">The request type that carries a <see cref="JsonElement"/> event and implements <see cref="IPreventExecutionOnLocalEnvironment"/>.</typeparam>
/// <typeparam name="TResponse">The response type derived from <see cref="Response"/>.</typeparam>
public class LocalEnvironmentWithEventBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BaseEventRequest<JsonElement>, IPreventExecutionOnLocalEnvironment
    where TResponse : Response
{
    /// <summary>
    /// Intercepts the request pipeline and prints the event JSON to the console when a debugger is attached, bypassing the actual handler.
    /// </summary>
    /// <param name="request">The incoming MediatR request containing the event payload.</param>
    /// <param name="next">The delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token to observe for cancellation of the operation.</param>
    /// <returns>A successful <see cref="Response"/> when debugging locally; otherwise the result of the next handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (Debugger.IsAttached)
        {
            var json = JsonSerializer.Serialize(request.Event, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n\n{json}\n\n");
            Console.ResetColor();

            return (TResponse)new Response(ResultEnum.SUCCESS);
        }

        return await next();
    }
}

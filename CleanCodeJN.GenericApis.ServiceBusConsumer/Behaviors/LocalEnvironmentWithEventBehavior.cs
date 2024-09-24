using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Behaviors;
public class LocalEnvironmentWithEventBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BaseEventRequest<JsonElement>, IPreventExecutionOnLocalEnvironment
    where TResponse : Response
{
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

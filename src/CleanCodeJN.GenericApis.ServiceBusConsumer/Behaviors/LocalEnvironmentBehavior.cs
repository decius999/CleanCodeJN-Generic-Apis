using System.Diagnostics;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Behaviors;
public class LocalEnvironmentBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPreventExecutionOnLocalEnvironment
    where TResponse : Response
{
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

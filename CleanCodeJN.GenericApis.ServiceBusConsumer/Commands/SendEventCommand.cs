using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
public class SendEventCommand(IServiceBusSenderCreator serviceBusSenderCreator) : IRequestHandler<SendEventRequest, Response>
{
    public async Task<Response> Handle(SendEventRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await serviceBusSenderCreator
                .GetOrCreateSender(request.TopicName)
                .SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(request.Event.ToJson()))
                {
                    ScheduledEnqueueTime = request.EnqueueTime,
                    MessageId = request.Event.InstanceId
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            return new Response(ResultEnum.FAILURE_INTERNAL_SERVER_ERROR, ex.StackTrace);
        }

        return new Response(ResultEnum.SUCCESS);
    }
}

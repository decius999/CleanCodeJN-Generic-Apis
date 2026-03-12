using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;

/// <summary>
/// MediatR handler that serializes an event and publishes it to the configured Service Bus topic.
/// </summary>
public class SendEventCommand(IServiceBusSenderCreator serviceBusSenderCreator) : IRequestHandler<SendEventRequest, Response>
{
    /// <summary>
    /// Handles the <see cref="SendEventRequest"/> by sending the event message to the Service Bus topic.
    /// </summary>
    /// <param name="request">The request containing the event payload and target topic information.</param>
    /// <param name="cancellationToken">A token to observe for cancellation of the operation.</param>
    /// <returns>A <see cref="Response"/> indicating success or failure of the send operation.</returns>
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

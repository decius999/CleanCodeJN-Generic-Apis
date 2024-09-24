using System.Text.Json;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
public class SendEventRequest : BaseEventRequest<JsonElement>, IRequest<Response>, IPreventExecutionOnLocalEnvironment
{
    public SendEventRequest(JsonElement oneEvent, string topicName, DateTime? enqueueTime = null, bool useBlobStorageWithLargeEvents = false) : base(oneEvent)
    {
        TopicName = topicName;
        EnqueueTime = enqueueTime.HasValue ? enqueueTime.Value : DateTime.UtcNow;
        UseBlobStorageWithLargeEvents = useBlobStorageWithLargeEvents;
    }

    public string TopicName { get; }

    public DateTimeOffset EnqueueTime { get; }

    public bool UseBlobStorageWithLargeEvents { get; }
}

using System.Text.Json;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;

/// <summary>
/// MediatR request that sends a <see cref="JsonElement"/> event to a specified Service Bus topic.
/// </summary>
public class SendEventRequest : BaseEventRequest<JsonElement>, IRequest<Response>, IPreventExecutionOnLocalEnvironment
{
    /// <summary>
    /// Initializes a new instance of <see cref="SendEventRequest"/> with the event payload, target topic, and optional scheduling details.
    /// </summary>
    /// <param name="oneEvent">The event payload as a <see cref="JsonElement"/>.</param>
    /// <param name="topicName">The name of the Service Bus topic to publish the event to.</param>
    /// <param name="enqueueTime">The UTC time at which the message should become available; defaults to now if not specified.</param>
    /// <param name="useBlobStorageWithLargeEvents">Indicates whether large event payloads should be offloaded to blob storage.</param>
    public SendEventRequest(JsonElement oneEvent, string topicName, DateTime? enqueueTime = null, bool useBlobStorageWithLargeEvents = false) : base(oneEvent)
    {
        TopicName = topicName;
        EnqueueTime = enqueueTime.HasValue ? enqueueTime.Value : DateTime.UtcNow;
        UseBlobStorageWithLargeEvents = useBlobStorageWithLargeEvents;
    }

    /// <summary>
    /// Gets the name of the Service Bus topic to which the event will be sent.
    /// </summary>
    public string TopicName { get; }

    /// <summary>
    /// Gets the scheduled enqueue time for the Service Bus message.
    /// </summary>
    public DateTimeOffset EnqueueTime { get; }

    /// <summary>
    /// Gets a value indicating whether large event payloads should be stored in blob storage instead of being sent inline.
    /// </summary>
    public bool UseBlobStorageWithLargeEvents { get; }
}

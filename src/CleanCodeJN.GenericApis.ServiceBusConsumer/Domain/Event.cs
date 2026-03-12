using System.Text.Json;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Domain;

/// <summary>
/// Represents a strongly-typed Service Bus event envelope carrying metadata and a typed payload.
/// </summary>
/// <typeparam name="T">The type of the event data payload.</typeparam>
public class Event<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="Event{T}"/> with the provided metadata and data payload.
    /// </summary>
    /// <param name="name">The human-readable name of the event.</param>
    /// <param name="type">The fully qualified type name used to route the event to its handler.</param>
    /// <param name="createdFrom">The service or component that originated the event.</param>
    /// <param name="data">The strongly-typed event payload.</param>
    /// <param name="topic">The Service Bus topic associated with this event.</param>
    /// <param name="requestId">An optional correlation identifier for tracing the originating request.</param>
    public Event(string name, string type, string createdFrom, T data, string topic, string requestId = null)
    {
        InstanceId = Guid.NewGuid().ToString();
        Name = name;
        Type = type;
        Environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        CreatedFrom = createdFrom;
        RequestId = requestId;
        Topic = topic;
        Data = data;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this event instance, including retry suffixes when applicable.
    /// </summary>
    public string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name of the event.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified type name used to resolve the correct MediatR handler for this event.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the runtime environment in which the event was created (e.g., Development, Production).
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// Gets or sets the timestamp at which the event was created, formatted as "yyyy-MM-dd HH:mm:ss".
    /// </summary>
    public string CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the name of the service or component that originated the event.
    /// </summary>
    public string CreatedFrom { get; set; }

    /// <summary>
    /// Gets or sets an optional correlation identifier linking this event to its originating HTTP request.
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the Service Bus topic name associated with this event.
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Gets or sets the number of times this event has been retried after a processing failure.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the strongly-typed payload of the event.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Serializes the event to a <see cref="JsonElement"/> representation.
    /// </summary>
    /// <returns>A <see cref="JsonElement"/> containing the full event data.</returns>
    public JsonElement ToJson() => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(this));
}

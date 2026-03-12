using System.Text.Json;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Domain;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;

/// <summary>
/// Base class for MediatR requests that carry a deserialized <see cref="Event{T}"/> payload extracted from a raw JSON message body.
/// </summary>
/// <typeparam name="T">The type of the event data payload.</typeparam>
public class BaseEventRequest<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="BaseEventRequest{T}"/> by deserializing the event from the provided <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="root">The root <see cref="JsonElement"/> representing the serialized event envelope.</param>
    public BaseEventRequest(JsonElement root) => Event = JsonSerializer.Deserialize<Event<T>>(root);

    /// <summary>
    /// Gets the deserialized event envelope containing metadata and the typed payload.
    /// </summary>
    public Event<T> Event { get; }
}

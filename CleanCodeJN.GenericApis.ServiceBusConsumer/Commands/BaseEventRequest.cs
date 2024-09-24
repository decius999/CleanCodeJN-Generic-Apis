using System.Text.Json;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Domain;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
public class BaseEventRequest<T>
{
    public BaseEventRequest(JsonElement root) => Event = JsonSerializer.Deserialize<Event<T>>(root);

    public Event<T> Event { get; }
}

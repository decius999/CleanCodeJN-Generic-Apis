using System.Text.Json;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Domain;
public class Event<T>
{
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

    public string InstanceId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Environment { get; set; }
    public string Domain { get; set; }
    public string CreatedAt { get; set; }
    public string CreatedFrom { get; set; }
    public string RequestId { get; set; }
    public string Topic { get; set; }
    public int RetryCount { get; set; }
    public T Data { get; set; }

    public JsonElement ToJson() => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(this));
}

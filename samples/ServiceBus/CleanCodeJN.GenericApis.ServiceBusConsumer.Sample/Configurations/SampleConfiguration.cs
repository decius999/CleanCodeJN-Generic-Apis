using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Configurations;

/// <summary>
/// Holds the top-level configuration settings for the service bus consumer sample application.
/// </summary>
public class SampleConfiguration
{
    /// <summary>
    /// Gets or sets the service bus configuration section, including connection and topic settings.
    /// </summary>
    public ServiceBusConfiguration ServiceBus { get; set; }
}

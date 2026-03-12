using System.Reflection;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample;

/// <summary>
/// Configures the service bus consumer for the sample application by providing connection settings and command assemblies.
/// </summary>
/// <param name="configuration">The monitored configuration containing service bus settings.</param>
/// <param name="logger">The logger instance for the configuration service.</param>
public class SampleServiceBusConsumerConfigurationService(
    IOptionsMonitor<SampleConfiguration> configuration,
    ILogger<SampleServiceBusConsumerConfigurationService> logger) : ServiceBusConsumerConfigurationServiceBase(logger)
{
    /// <summary>
    /// Returns the service bus topic configuration from the current application settings.
    /// </summary>
    /// <returns>The <see cref="ServiceBusConfiguration"/> containing connection and topic details.</returns>
    public override ServiceBusConfiguration GetServiceBusTopicConfiguration() => configuration.CurrentValue.ServiceBus;

    /// <summary>
    /// Returns the assemblies that contain MediatR command handlers for service bus events.
    /// </summary>
    /// <returns>A list of assemblies to scan for event command handlers.</returns>
    public override List<Assembly> GetCommandAssemblies() => [typeof(UpdateInvoiceEventRequest).Assembly];
}

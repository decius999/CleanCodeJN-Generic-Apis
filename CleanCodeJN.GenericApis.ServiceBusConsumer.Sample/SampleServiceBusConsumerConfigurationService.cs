using System.Reflection;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample;
public class SampleServiceBusConsumerConfigurationService(
    IOptionsMonitor<SampleConfiguration> configuration,
    ILogger<SampleServiceBusConsumerConfigurationService> logger) : ServiceBusConsumerConfigurationServiceBase(logger)
{
    public override ServiceBusConfiguration GetServiceBusTopicConfiguration() => configuration.CurrentValue.ServiceBus;

    public override List<Assembly> GetCommandAssemblies() => [typeof(UpdateInvoiceEventRequest).Assembly];
}

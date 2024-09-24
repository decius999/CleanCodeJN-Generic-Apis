using System.Reflection;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Behaviors;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
public static class DIExtensions
{
    /// <summary>
    /// Register Service Bus Consumer Services
    /// </summary>
    /// <typeparam name="TServiceBusConsumerConfigurationService">Impementation of IServiceBusConsumerConfigurationService</typeparam>
    /// <param name="services">Builder.Services</param>
    /// <param name="serviceBusConnectionString">Azure Service Bus Connectionstring</param>
    /// <param name="commandAssemblies">Assemblies where Commands are implemented which should be called during event processing</param>
    public static void RegisterServiceBusConsumer<TServiceBusConsumerConfigurationService>(this IServiceCollection services, string serviceBusConnectionString, List<Assembly> commandAssemblies)
        where TServiceBusConsumerConfigurationService : IServiceBusConsumerConfigurationService
    {
        services.AddTransient<ICommandExecutionContext, CommandExecutionContext>();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(commandAssemblies.ToArray());
            config.AddOpenBehavior(typeof(LocalEnvironmentWithEventBehavior<,>));
            config.AddOpenBehavior(typeof(LocalEnvironmentBehavior<,>));
            config.Lifetime = ServiceLifetime.Scoped;
        });

        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(serviceBusConnectionString);
        });

        services.AddSingleton<IServiceBusSenderCreator, ServiceBusSenderCreator>();
        services.AddSingleton(typeof(IServiceBusConsumerConfigurationService), typeof(TServiceBusConsumerConfigurationService));
        services.AddHostedService<ServiceBusConsumerBackendService>();
    }
}

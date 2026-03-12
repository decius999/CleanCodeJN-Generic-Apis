using System.Reflection;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Behaviors;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Domain;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using CleanCodeJN.Repository.EntityFramework.Extensions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
public static class ServiveCollectionExtensions
{
    /// <summary>
    /// Register Service Bus Consumer Services
    /// </summary>
    /// <typeparam name="TServiceBusConsumerConfigurationService">Impementation of IServiceBusConsumerConfigurationService</typeparam>
    /// <typeparam name="TDataContext">Your DbContext type</typeparam>
    /// <param name="services">IServiceCollection services</param>
    /// <param name="optionAction">The option Action to configure the package</param>
    /// <returns></returns>
    public static IServiceCollection AddCleanCodeJNServiceBusConsumer<TServiceBusConsumerConfigurationService, TDataContext>(
        this IServiceCollection services, Action<CleanCodeOptionsServiceBusConsumer> optionAction)
            where TServiceBusConsumerConfigurationService : IServiceBusConsumerConfigurationService
            where TDataContext : class, IDataContext
    {
        var options = new CleanCodeOptionsServiceBusConsumer();
        optionAction.Invoke(options);

        services.RegisterValidatorsFromAssembly(options.ValidatorAssembly)
            .RegisterGenericCommands(options.ApplicationAssemblies)
            .RegisterAutomapper(options.ApplicationAssemblies)
            .RegisterServiceBusConsumer<TServiceBusConsumerConfigurationService>(options.ServiceBusConnectionString, options.ApplicationAssemblies)
            .RegisterDbContextAndRepositories<TDataContext>();

        return services;
    }

    /// <summary>
    /// Register Service Bus Consumer Services
    /// </summary>
    /// <typeparam name="TServiceBusConsumerConfigurationService">Impementation of IServiceBusConsumerConfigurationService</typeparam>
    /// <param name="services">Builder.Services</param>
    /// <param name="serviceBusConnectionString">Azure Service Bus Connectionstring</param>
    /// <param name="commandAssemblies">Assemblies where Commands are implemented which should be called during event processing</param>
    private static IServiceCollection RegisterServiceBusConsumer<TServiceBusConsumerConfigurationService>(this IServiceCollection services, string serviceBusConnectionString, List<Assembly> commandAssemblies)
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

        return services;
    }
}

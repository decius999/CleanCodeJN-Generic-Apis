using System.Reflection;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.DataAccess;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Extensions;
using CleanCodeJN.Repository.EntityFramework.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

List<Assembly> assemblies = [
    typeof(CleanCodeJN.GenericApis.Sample.Business.AssemblyRegistration).Assembly,
    typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly,
    typeof(CleanCodeJN.GenericApis.Sample.Domain.AssemblyRegistration).Assembly,
    Assembly.GetExecutingAssembly(),
];

builder.Services.Configure<SampleConfiguration>(builder.Configuration);

builder.Services
            .RegisterValidatorsFromAssembly(typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly)
            .RegisterGenericCommands(assemblies)
            .RegisterAutomapper(assemblies)
            .RegisterServiceBusConsumer<SampleServiceBusConsumerConfigurationService>(builder.Configuration["ServiceBus:ConnectionString"], assemblies)
            .RegisterDbContextAndRepositories<MyDbContext>();

var app = builder.Build();

app.EnsureDatabaseCreated();

await app.RunAsync();

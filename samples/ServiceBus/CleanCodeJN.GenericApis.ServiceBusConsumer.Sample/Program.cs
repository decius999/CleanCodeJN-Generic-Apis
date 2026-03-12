using System.Reflection;
using CleanCodeJN.GenericApis.Sample.DataAccess;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SampleConfiguration>(builder.Configuration);

builder.Services.AddCleanCodeJNServiceBusConsumer<SampleServiceBusConsumerConfigurationService, MyDbContext>(options =>
{
    options.ApplicationAssemblies =
    [
        typeof(CleanCodeJN.GenericApis.Sample.Business.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Domain.AssemblyRegistration).Assembly,
        Assembly.GetExecutingAssembly(),
    ];
    options.ServiceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];
});

var app = builder.Build();

app.EnsureDatabaseCreated();

await app.RunAsync();

using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SampleConfiguration>(builder.Configuration);

builder.Services.RegisterServiceBusConsumer<SampleServiceBusConsumerConfigurationService>(
   builder.Configuration["ServiceBus:ConnectionString"], [typeof(UpdateInvoiceEventRequest).Assembly]);

await builder.Build().RunAsync();

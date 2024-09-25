using CleanCodeJN.GenericApis.Sample.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Extensions;
public static class HostExtensions
{
    public static void EnsureDatabaseCreated(this IHost app)
    {
        using var serviceScope = app.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<MyDbContext>();
        context?.Database?.EnsureCreated();
    }
}

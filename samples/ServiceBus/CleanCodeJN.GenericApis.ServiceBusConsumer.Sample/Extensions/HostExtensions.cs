using CleanCodeJN.GenericApis.Sample.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IHost"/> to support database initialization.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Ensures the database is created, applying any pending schema creation if necessary.
    /// </summary>
    /// <param name="app">The <see cref="IHost"/> instance used to resolve the database context.</param>
    public static void EnsureDatabaseCreated(this IHost app)
    {
        using var serviceScope = app.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<MyDbContext>();
        context?.Database?.EnsureCreated();
    }
}

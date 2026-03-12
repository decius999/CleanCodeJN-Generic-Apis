using CleanCodeJN.GenericApis.Sample.DataAccess;

namespace CleanCodeJN.GenericApis.Sample.Extensions;

/// <summary>
/// Provides extension methods for <see cref="WebApplication"/> to support database initialization.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Ensures the database is created, applying any pending schema creation if necessary.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance used to resolve the database context.</param>
    public static void EnsureDatabaseCreated(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<MyDbContext>();
        context?.Database?.EnsureCreated();
    }
}

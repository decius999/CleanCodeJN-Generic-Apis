using CleanCodeJN.GenericApis.Sample.Context;

namespace CleanCodeJN.GenericApis.Sample.Extensions;

public static class WebApplicationExtensions
{
    public static void EnsureDatabaseCreated(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<MyDbContext>();
        context?.Database?.EnsureCreated();
    }
}

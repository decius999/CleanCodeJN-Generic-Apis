using CleanCodeJN.GenericApis.DataGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace CleanCodeJN.GenericApis.DataGrid.Extensions;

/// <summary>Extension methods for registering the DataGrid services in the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="GraphQLDataGridService"/> and the named <c>CleanCodeJN.DataGrid</c>
    /// <see cref="System.Net.Http.HttpClient"/> required by <c>CCJNDataGrid</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddCleanCodeJNDataGrid();
    /// </code>
    /// </example>
    public static IServiceCollection AddCleanCodeJNDataGrid(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddHttpClient("CleanCodeJN.DataGrid");
        services.AddScoped<GraphQLDataGridService>();
        return services;
    }
}

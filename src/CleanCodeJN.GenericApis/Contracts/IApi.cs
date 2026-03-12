namespace CleanCodeJN.GenericApis.Contracts;

/// <summary>
/// The minimal API interface
/// </summary>
public interface IApi
{
    /// <summary>
    /// The Swagger/OpenAPI Tags
    /// </summary>
    List<string> Tags { get; }

    /// <summary>
    /// The endpoint base route
    /// </summary>
    string Route { get; }

    /// <summary>
    /// The minimal API HTTP Methods to be mapped
    /// </summary>
    List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods { get; }
}

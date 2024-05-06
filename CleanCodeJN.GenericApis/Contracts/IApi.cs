namespace CleanCodeJN.GenericApis.Contracts;

public interface IApi
{
    List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods { get; }

    string Route { get; }

    List<string> Tags { get; }
}

using HotChocolate.Execution;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// A GraphQL error filter that logs schema and execution errors to the console during startup.
/// </summary>
public class GraphQLStartupErrorFilter : IErrorFilter
{
    /// <summary>
    /// Intercepts a GraphQL error, writes its details to the console in red, and returns the error unchanged.
    /// </summary>
    /// <param name="error">The GraphQL error to handle.</param>
    /// <returns>The original <see cref="IError"/> instance.</returns>
    public IError OnError(IError error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("GraphQL Schema Error:");
        Console.WriteLine(error.Exception?.Message);
        Console.WriteLine(error.Exception?.StackTrace);
        Console.ResetColor();

        return error;
    }
}


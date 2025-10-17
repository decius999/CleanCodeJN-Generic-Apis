namespace CleanCodeJN.GenericApis.Extensions;

public class GraphQLStartupErrorFilter : IErrorFilter
{
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


namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;

/// <summary>
/// Provides string utility extension methods for the Service Bus Consumer package.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Trims surrounding quotes from the string and returns the base instance identifier before the first underscore.
    /// </summary>
    /// <param name="str">The instance identifier string to trim.</param>
    /// <returns>The base portion of the instance identifier without retry suffixes.</returns>
    public static string TrimInstanceId(this string str) => str.Trim('"').Split('_')[0];

    /// <summary>
    /// Prints the CleanCode JN ASCII art logo to the console in blue.
    /// </summary>
    public static void PrintLogo()
    {
        string[] asciiArt =
        {
            @"    _____ _                     _____          _             _ _   _   ______               _          ",
            @"   / ____| |                   / ____|        | |           | | \ | | |  ____|             | |         ",
            @"  | |    | | ___  __ _ _ __   | |     ___   __| | ___       | |  \| | | |____   _____ _ __ | |_ ___    ",
            @"  | |    | |/ _ \/ _` | '_ \  | |    / _ \ / _` |/ _ \  _   | | . ` | |  __\ \ / / _ \ '_ \| __/ __|   ",
            @"  | |____| |  __/ (_| | | | | | |___| (_) | (_| |  __/ | |__| | |\  | | |___\ V /  __/ | | | |_\__ \   ",
            @"   \_____|_|\___|\__,_|_| |_|  \_____\___/ \__,_|\___|  \____/|_| \_| |______\_/ \___|_| |_|\__|___/   ",
        };

        foreach (var ascii in asciiArt)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(ascii);
        }

        Console.ResetColor();
    }
}

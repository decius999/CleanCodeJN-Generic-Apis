namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
public static class StringExtensions
{
    public static string TrimInstanceId(this string str) => str.Trim('"').Split('_')[0];

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

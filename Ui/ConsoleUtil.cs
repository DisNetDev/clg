internal static class ConsoleUtil
{
    public static void WriteWarning(string text)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }

    public static void PrintHeader(string title)
    {
        var prev = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Cyan;
        string[] logo = new[] {
@"    ___ _          __     _     ___                           ",
@"   /   (_)___   /\ \ \___| |_  / _ \__ _ _ __ ___   ___  ___  ",
@"  / /\ / / __| /  \/ / _ \ __|/ /_\/ _` | '_ ` _ \ / _ \/ __| ",
@" / /_//| \__ \/ /\  /  __/ |_/ /_\\ (_| | | | | | |  __/\__ \ ",
@"/___,' |_|___/\_\ \/ \___|\__\____/\__,_|_| |_| |_|\___||___/ ",
            };

        int width = 80;
        try
        {
            width = Console.WindowWidth;
        }
        catch
        {
            // ignore if not available
        }

        // Clear only the header region (logo + title + one blank line) so
        // the rest of the console content is preserved when navigating pages.
        int headerLines = logo.Length + 2;
        try
        {
            Console.SetCursorPosition(0, 0);
            string blankLine = new string(' ', Math.Max(width, 1));
            for (int i = 0; i < headerLines; i++)
            {
                Console.WriteLine(blankLine);
            }
            Console.SetCursorPosition(0, 0);
        }
        catch
        {
            // Fallback to clearing the whole console if positioning isn't supported
            try { Console.Clear(); } catch { }
        }

        foreach (var line in logo)
        {
            Console.WriteLine(CenterText(line, width));
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(CenterText(title, width));
        Console.ForegroundColor = prev;
        Console.WriteLine();
    }

    private static string CenterText(string text, int width)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (text.Length >= width) return text;
        int left = Math.Max((width - text.Length) / 2, 0);
        return new string(' ', left) + text;
    }
}

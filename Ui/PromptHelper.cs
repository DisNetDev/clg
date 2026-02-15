using Sharprompt;

internal static class PromptHelper
{
    // Clears the console before showing the prompt and clears again after selection.
    public static string? Select(string title, IEnumerable<string> options)
    {
        Console.Clear();
        try
        {
            var sel = Prompt.Select(title, options);
            Console.Clear();
            return sel;
        }
        catch (Exception ex) when (PromptUtil.IsPromptCancellation(ex))
        {
            Console.Clear();
            return null;
        }
    }

    // Assumes the caller already printed any header/description they want preserved.
    // Does not clear before prompt, but clears after selection.
    public static string? SelectPreserveDisplay(string title, IEnumerable<string> options)
    {
        try
        {
            var sel = Prompt.Select(title, options);
            Console.Clear();
            return sel;
        }
        catch (Exception ex) when (PromptUtil.IsPromptCancellation(ex))
        {
            Console.Clear();
            return null;
        }
    }

    public static string? Input(string prompt)
    {
        try
        {
            return Prompt.Input<string>(prompt);
        }
        catch (Exception ex) when (PromptUtil.IsPromptCancellation(ex))
        {
            return null;
        }
    }

    public static bool? Confirm(string prompt, bool defaultValue = false)
    {
        try
        {
            return Prompt.Confirm(prompt, defaultValue: defaultValue);
        }
        catch (Exception ex) when (PromptUtil.IsPromptCancellation(ex))
        {
            return null;
        }
    }
}

internal static class PromptUtil
{
    public static bool IsPromptCancellation(Exception ex)
    {
        var name = ex.GetType().Name;
        return name.Contains("Cancel", StringComparison.OrdinalIgnoreCase)
               || name.Contains("Abort", StringComparison.OrdinalIgnoreCase);
    }
}

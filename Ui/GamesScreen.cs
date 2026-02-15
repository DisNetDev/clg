using Sharprompt;

internal static class GamesScreen
{
    public static void Show(string configPath)
    {
        const string backOption = "Back";

        while (true)
        {
            Console.Clear();

            var config = ConfigStore.Load(configPath);
            var options = config.Games
                .Select(g => g.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            options.Add(backOption);

            var selected = PromptHelper.SelectPreserveDisplay("Games", options);
            if (selected is null || selected == backOption)
            {
                return;
            }

            // Selecting a game currently just stays on this screen.
        }
    }
}

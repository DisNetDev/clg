using Sharprompt;
using System.Diagnostics;
using System.IO;

internal static class GamesScreen
{
    public static void Show(string configPath)
    {
        const string backOption = "Back";

        while (true)
        {
            ConsoleUtil.PrintHeader("Games");

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

            var game = config.Games.FirstOrDefault(g => string.Equals(g.Name, selected, StringComparison.OrdinalIgnoreCase));
            if (game is null)
            {
                continue;
            }

            var actionOptions = new List<string> { "Launch", "Change executable path", backOption };
            var action = PromptHelper.SelectPreserveDisplay("Action", actionOptions);
            if (action is null || action == backOption)
            {
                continue;
            }

            if (action == "Change executable path")
            {
                var newPath = ChooseExecutableForGame(game);
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    var idx = config.Games.FindIndex(g => string.Equals(g.Name, game.Name, StringComparison.OrdinalIgnoreCase));
                    if (idx >= 0)
                    {
                        config.Games[idx] = config.Games[idx] with { ExecutablePath = newPath };
                        ConfigStore.Save(configPath, config);
                        Console.WriteLine("Executable path updated. Press any key to continue.");
                        Console.ReadKey(true);
                    }
                }

                continue;
            }

            // action == "Launch"
            var execPath = (game.ExecutablePath ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(execPath) || !File.Exists(execPath))
            {
                var chosen = ChooseExecutableForGame(game);
                if (string.IsNullOrWhiteSpace(chosen))
                {
                    continue;
                }

                execPath = chosen;
                var idx = config.Games.FindIndex(g => string.Equals(g.Name, game.Name, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    config.Games[idx] = config.Games[idx] with { ExecutablePath = execPath };
                    ConfigStore.Save(configPath, config);
                }
            }

            try
            {
                var psi = new ProcessStartInfo(execPath) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to launch '{execPath}': {ex.Message}");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey(true);
            }
        }
    }

    private static string? ChooseExecutableForGame(DNGEntry game)
    {
        List<string> exeFiles = new();
        try
        {
            if (!Directory.Exists(game.Path))
            {
                Console.WriteLine("Game path not found. Press any key to continue.");
                Console.ReadKey(true);
                return null;
            }

            exeFiles = Directory.EnumerateFiles(game.Path, "*.exe", SearchOption.AllDirectories)
                .OrderBy(f => f)
                .ToList();
        }
        catch
        {
            Console.WriteLine("Could not enumerate executables in the game folder. Press any key to continue.");
            Console.ReadKey(true);
            return null;
        }

        if (exeFiles.Count == 0)
        {
            Console.WriteLine("No .exe files found in the game folder. Press any key to continue.");
            Console.ReadKey(true);
            return null;
        }

        var choice = PromptHelper.SelectPreserveDisplay("Select executable", exeFiles);
        if (string.IsNullOrWhiteSpace(choice))
        {
            return null;
        }

        return choice;
    }
}

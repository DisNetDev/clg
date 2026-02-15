using System.IO;
using System.Linq;
using Sharprompt;

class Program
{
    static void Main(string[] args)
    {
        var configPath = ConfigStore.EnsureConfigExists();

        try
        {
            var added = ConfigStore.PopulateGamesFromMonitoredPaths(configPath);
            if (added)
            {
                Console.WriteLine("Detected games from monitored paths and updated config.");
            }
        }
        catch
        {
            // ignore scanning errors at startup
        }

        while (true)
        {
            Console.Clear();

            var config = ConfigStore.Load(configPath);

            if (config.PathsToMonitor.Count == 0)
            {
                Console.WriteLine("No paths are being monitored yet.");
                Console.WriteLine("Open 'Game Paths to Monitor' to add your first path.");
                Console.WriteLine();
            }

            var missing = config.PathsToMonitor
                .Where(p => !Directory.Exists(p) && !Directory.Exists(p.Replace('\\', Path.DirectorySeparatorChar)))
                .ToList();

            if (missing.Any())
            {
                ConsoleUtil.WriteWarning($"Warning: {missing.Count} monitored path(s) do not exist:");
                foreach (var m in missing)
                {
                    Console.WriteLine(" - " + m);
                }
                Console.WriteLine();
            }

            var selection = PromptHelper.SelectPreserveDisplay("Select", new[] { "Games", "Game Paths to Monitor" });
            if (selection is null)
            {
                return;
            }

            switch (selection)
            {
                case "Game Paths to Monitor":
                    PathsToMonitorScreen.Show(configPath);
                    break;
                case "Games":
                    GamesScreen.Show(configPath);
                    break;
                default:
                    Console.WriteLine($"Selected: {selection}");
                    break;
            }
        }
    }
}

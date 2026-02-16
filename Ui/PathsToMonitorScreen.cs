using Sharprompt;
using System.IO;
using System;
using System.Collections.Generic;

internal static class PathsToMonitorScreen
{
    public static void Show(string configPath)
    {
        const string addOption = "Add new path";
        const string backOption = "Back";
        var reserved = new[] { addOption, backOption };

        while (true)
        {
            ConsoleUtil.PrintHeader("Paths to Monitor");

            var config = ConfigStore.Load(configPath);
            if (config.PathsToMonitor.Count == 0)
            {
                Console.WriteLine("No paths are being monitored yet. Add your first path below.");
                Console.WriteLine();
            }
            var options = new List<string>(config.PathsToMonitor);
            options.Add(addOption);
            options.Add(backOption);

            var selected = PromptHelper.SelectPreserveDisplay("Paths to monitor", options);
            if (selected is null)
            {
                return;
            }

            if (selected == backOption)
            {
                return;
            }

            if (selected != addOption)
            {
                var delete = PromptHelper.Confirm($"Delete '{selected}'?", defaultValue: false);
                if (delete is null)
                {
                    continue;
                }

                ConsoleUtil.PrintHeader("Paths to Monitor");

                if (delete == false)
                {
                    continue;
                }

                if (config.PathsToMonitor.Remove(selected))
                {
                    try
                    {
                        var selNorm = Path.GetFullPath(selected)
                            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Replace('/', '\\');

                        config.Games.RemoveAll(g =>
                        {
                            try
                            {
                                var gp = Path.GetFullPath(g.Path ?? string.Empty).Replace('/', '\\');
                                return string.Equals(gp, selNorm, StringComparison.OrdinalIgnoreCase)
                                    || gp.StartsWith(selNorm + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                            }
                            catch
                            {
                                var gpRaw = (g.Path ?? string.Empty).Replace('/', '\\')
                                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                return gpRaw.StartsWith(selNorm, StringComparison.OrdinalIgnoreCase);
                            }
                        });
                    }
                    catch
                    {
                        var selRaw = selected.Replace('/', '\\').TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        config.Games.RemoveAll(g => (g.Path ?? string.Empty).Replace('/', '\\').StartsWith(selRaw, StringComparison.OrdinalIgnoreCase));
                    }

                    ConfigStore.Save(configPath, config);
                }

                continue;
            }

            while (true)
            {
                var input = PromptHelper.Input("New path (blank/Back to cancel)") ?? string.Empty;
                var newPath = input.Trim();

                ConsoleUtil.PrintHeader("Paths to Monitor");

                if (string.IsNullOrWhiteSpace(newPath)
                    || string.Equals(newPath, backOption, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (!WindowsPathValidator.IsValidWindowsPath(newPath, reserved, out var error))
                {
                    Console.Error.WriteLine(error);
                    continue;
                }

                config.PathsToMonitor.Add(newPath);
                ConfigStore.Save(configPath, config);
                try
                {
                    var addedGames = ConfigStore.PopulateGamesFromMonitoredPaths(configPath);
                    if (addedGames)
                    {
                        Console.WriteLine("Scanned new path and updated games.");
                    }
                }
                catch
                {
                    // ignore scanning errors for UI flow
                }
                break;
            }
        }
    }
}

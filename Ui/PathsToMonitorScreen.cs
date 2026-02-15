using Sharprompt;

internal static class PathsToMonitorScreen
{
    public static void Show(string configPath)
    {
        const string addOption = "Add new path";
        const string backOption = "Back";
        var reserved = new[] { addOption, backOption };

        while (true)
        {
            Console.Clear();

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

                Console.Clear();

                if (delete == false)
                {
                    continue;
                }

                if (config.PathsToMonitor.Remove(selected))
                {
                    ConfigStore.Save(configPath, config);
                }

                continue;
            }

            while (true)
            {
                var input = PromptHelper.Input("New path (blank/Back to cancel)") ?? string.Empty;
                var newPath = input.Trim();

                Console.Clear();

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
                break;
            }
        }
    }
}

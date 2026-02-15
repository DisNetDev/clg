using System.Text.Json;
using System.IO;
using System.Linq;

internal static class ConfigStore
{
    public const string ConfigFileName = "clg.json";

    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static string EnsureConfigExists()
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);

        if (File.Exists(configPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
                if (IsValidConfigObject(doc.RootElement))
                {
                    return configPath;
                }

                if (TryReadConfigFromOldFormats(doc.RootElement, out var migrated))
                {
                    Save(configPath, migrated);
                    Console.WriteLine($"Updated config format: {configPath}");
                    return configPath;
                }
            }
            catch
            {
                // Fall through and rewrite a valid default config.
            }
        }

        Save(configPath, new ClgConfig
        {
            Games = new(),
            PathsToMonitor = new(),
        });

        Console.WriteLine($"Created default config: {configPath}");
        return configPath;
    }

    public static ClgConfig Load(string configPath)
    {
        try
        {
            var json = File.ReadAllText(configPath);
            var loaded = JsonSerializer.Deserialize<ClgConfig>(json, ConfigJsonOptions);
            return Normalize(loaded);
        }
        catch
        {
            return new ClgConfig { Games = new(), PathsToMonitor = new() };
        }
    }

    private static ClgConfig Normalize(ClgConfig? config)
    {
        if (config is null)
        {
            return new ClgConfig { Games = new(), PathsToMonitor = new() };
        }

        var games = config.Games ?? new List<ClgEntry>();
        var paths = config.PathsToMonitor ?? new List<string>();

        var normalizedGames = new List<ClgEntry>(games.Count);
        foreach (var entry in games)
        {
            if (entry is null)
            {
                continue;
            }

            normalizedGames.Add(new ClgEntry
            {
                Name = entry.Name ?? string.Empty,
                Path = entry.Path ?? string.Empty,
                ExecutablePath = (entry as dynamic)?.ExecutablePath ?? string.Empty,
            });
        }

        var normalizedPaths = new List<string>(paths.Count);
        foreach (var path in paths)
        {
            normalizedPaths.Add(path ?? string.Empty);
        }

        return new ClgConfig
        {
            Games = normalizedGames,
            PathsToMonitor = normalizedPaths,
        };
    }

    public static void Save(string configPath, ClgConfig config)
    {
        var json = JsonSerializer.Serialize(config, ConfigJsonOptions);

        try
        {
            File.WriteAllText(configPath, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: could not write config at '{configPath}': {ex.Message}");
        }
    }

    public static bool PopulateGamesFromMonitoredPaths(string configPath)
    {
        var config = Load(configPath);
        var existingNames = new HashSet<string>(config.Games.Select(g => g.Name), StringComparer.OrdinalIgnoreCase);
        var added = false;
        var removedAny = false;

        // Remove games whose stored path no longer exists
        var existingGames = config.Games.ToList();
        foreach (var g in existingGames)
        {
            var p = g.Path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(p))
            {
                continue;
            }

            var exists = Directory.Exists(p) || Directory.Exists(p.Replace('\\', Path.DirectorySeparatorChar));
            if (!exists)
            {
                config.Games.Remove(g);
                removedAny = true;
            }
        }

        foreach (var monitorPath in config.PathsToMonitor)
        {
            if (string.IsNullOrWhiteSpace(monitorPath))
            {
                continue;
            }

            try
            {
                if (!Directory.Exists(monitorPath))
                {
                    continue;
                }

                foreach (var dir in Directory.GetDirectories(monitorPath))
                {
                    var name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    if (!existingNames.Contains(name))
                    {
                        // store the discovered game's folder full path normalized to Windows style
                        var full = Path.GetFullPath(dir);
                        var windowsPath = full.Replace('/', '\\');
                        config.Games.Add(new ClgEntry { Name = name, Path = windowsPath, ExecutablePath = string.Empty });
                        existingNames.Add(name);
                        added = true;
                    }
                }
            }
            catch
            {
                // ignore inaccessible monitor paths
                continue;
            }
        }

        if (added || removedAny)
        {
            Save(configPath, config);
        }

        return added || removedAny;
    }

    private static bool IsValidConfigObject(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!root.TryGetProperty("games", out var gamesProp) || gamesProp.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var element in gamesProp.EnumerateArray())
        {
            if (!TryReadSingleEntry(element, out _))
            {
                return false;
            }
        }

        if (!root.TryGetProperty("pathsToMonitor", out var pathsProp) || pathsProp.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var element in pathsProp.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryReadConfigFromOldFormats(JsonElement root, out ClgConfig config)
    {
        // Old format #1: top-level array of entries
        if (root.ValueKind == JsonValueKind.Array)
        {
            var entries = new List<ClgEntry>();
            foreach (var element in root.EnumerateArray())
            {
                if (!TryReadSingleEntry(element, out var entry))
                {
                    config = default!;
                    return false;
                }

                entries.Add(entry);
            }

            config = new ClgConfig { Games = entries, PathsToMonitor = new() };
            return true;
        }

        // Old format #2: single entry object { Name, Path }
        if (TryReadSingleEntry(root, out var singleEntry))
        {
            config = new ClgConfig { Games = new() { singleEntry }, PathsToMonitor = new() };
            return true;
        }

        // Partial/new-ish: object with one of the lists present
        if (root.ValueKind == JsonValueKind.Object)
        {
            var games = new List<ClgEntry>();
            var pathsToMonitor = new List<string>();

            if (root.TryGetProperty("games", out var gamesProp) && gamesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in gamesProp.EnumerateArray())
                {
                    if (!TryReadSingleEntry(element, out var entry))
                    {
                        config = default!;
                        return false;
                    }
                    games.Add(entry);
                }
            }

            if (root.TryGetProperty("pathsToMonitor", out var pathsProp) && pathsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in pathsProp.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.String)
                    {
                        config = default!;
                        return false;
                    }
                    pathsToMonitor.Add(element.GetString() ?? string.Empty);
                }
            }

            if (games.Count > 0 || pathsToMonitor.Count > 0)
            {
                config = new ClgConfig { Games = games, PathsToMonitor = pathsToMonitor };
                return true;
            }
        }

        config = default!;
        return false;
    }

    private static bool TryReadSingleEntry(JsonElement element, out ClgEntry entry)
    {
        entry = default!;
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!TryGetStringProperty(element, "name", "Name", out var name))
        {
            return false;
        }

        if (!TryGetStringProperty(element, "path", "Path", out var path))
        {
            return false;
        }

        string execPath = string.Empty;
        TryGetStringProperty(element, "executablePath", "ExecutablePath", out execPath);

        entry = new ClgEntry { Name = name, Path = path, ExecutablePath = execPath };
        return true;
    }

    private static bool TryGetStringProperty(JsonElement obj, string camelCaseName, string pascalCaseName, out string value)
    {
        value = string.Empty;

        if (obj.TryGetProperty(camelCaseName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString() ?? string.Empty;
            return true;
        }

        if (obj.TryGetProperty(pascalCaseName, out prop) && prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString() ?? string.Empty;
            return true;
        }

        return false;
    }
}

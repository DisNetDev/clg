internal sealed record ClgConfig
{
    public required List<ClgEntry> Games { get; init; }
    public required List<string> PathsToMonitor { get; init; }
}

internal sealed record ClgEntry
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string ExecutablePath { get; init; }
}

internal sealed record DNGConfig
{
    public required List<DNGEntry> Games { get; init; }
    public required List<string> PathsToMonitor { get; init; }
}

internal sealed record DNGEntry
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string ExecutablePath { get; init; }
}

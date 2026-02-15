internal static class WindowsPathValidator
{
    public static bool IsValidWindowsPath(string input, IReadOnlyCollection<string> reservedValues, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Path is required.";
            return false;
        }

        if (reservedValues.Any(v => string.Equals(input, v, StringComparison.OrdinalIgnoreCase)))
        {
            error = "That value is reserved for the menu; choose a different path.";
            return false;
        }

        if (input.Contains('/'))
        {
            error = "Invalid Windows path: use \\\\ as the separator (not /).";
            return false;
        }

        foreach (var c in input)
        {
            if (c < 32)
            {
                error = "Invalid Windows path: contains control characters.";
                return false;
            }
        }

        if (input.StartsWith("\\\\"))
        {
            // UNC: \\server\share\...
            var uncRemainder = input[2..];
            var parts = uncRemainder.Split('\\', StringSplitOptions.None);
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                error = "Invalid UNC path: expected \\\\server\\share\\...";
                return false;
            }

            for (var i = 0; i < parts.Length; i++)
            {
                if (!IsValidWindowsPathSegment(parts[i], out error))
                {
                    return false;
                }
            }

            return true;
        }

        // Drive-absolute: C:\...
        if (input.Length >= 3 && char.IsLetter(input[0]) && input[1] == ':' && input[2] == '\\')
        {
            var remainder = input[3..];
            if (remainder.Length == 0)
            {
                return true;
            }

            var parts = remainder.Split('\\', StringSplitOptions.None);
            foreach (var part in parts)
            {
                if (!IsValidWindowsPathSegment(part, out error))
                {
                    return false;
                }
            }

            return true;
        }

        // Rooted-on-current-drive: \folder\...
        if (input.StartsWith("\\"))
        {
            var remainder = input[1..];
            if (remainder.Length == 0)
            {
                error = "Invalid Windows path: path root must be followed by a folder name.";
                return false;
            }

            var parts = remainder.Split('\\', StringSplitOptions.None);
            foreach (var part in parts)
            {
                if (!IsValidWindowsPathSegment(part, out error))
                {
                    return false;
                }
            }

            return true;
        }

        error = "Invalid Windows path: must start with a drive (e.g. C:\\Games\\...) or UNC (e.g. \\\\server\\share\\...).";
        return false;
    }

    private static bool IsValidWindowsPathSegment(string segment, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrEmpty(segment))
        {
            error = "Invalid Windows path: contains an empty path segment (double \\\\).";
            return false;
        }

        if (segment.EndsWith(' ') || segment.EndsWith('.'))
        {
            error = "Invalid Windows path: folder/file names cannot end with a space or dot.";
            return false;
        }

        var baseName = segment.Split('.', 2)[0];
        var upper = baseName.ToUpperInvariant();
        if (upper is "CON" or "PRN" or "AUX" or "NUL"
            or "COM1" or "COM2" or "COM3" or "COM4" or "COM5" or "COM6" or "COM7" or "COM8" or "COM9"
            or "LPT1" or "LPT2" or "LPT3" or "LPT4" or "LPT5" or "LPT6" or "LPT7" or "LPT8" or "LPT9")
        {
            error = $"Invalid Windows path: '{baseName}' is a reserved name.";
            return false;
        }

        foreach (var c in segment)
        {
            if (c is '<' or '>' or '"' or '|' or '?' or '*')
            {
                error = $"Invalid Windows path: contains invalid character '{c}'.";
                return false;
            }

            if (c == ':')
            {
                error = "Invalid Windows path: ':' is only allowed after the drive letter (e.g. C:\\...).";
                return false;
            }
        }

        return true;
    }
}

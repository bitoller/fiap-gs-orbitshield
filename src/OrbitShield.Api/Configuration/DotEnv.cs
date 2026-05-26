namespace OrbitShield.Api.Configuration;

public static class DotEnv
{
    public static void LoadFromNearest(string fileName, bool overrideExisting = false)
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        foreach (var directory in WalkParents(currentDirectory).Concat(WalkParents(baseDirectory)))
        {
            var filePath = Path.Combine(directory.FullName, fileName);
            if (File.Exists(filePath))
            {
                Load(filePath, overrideExisting);
                return;
            }
        }
    }

    public static void Load(string filePath, bool overrideExisting = false)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"');

            if (overrideExisting || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static IEnumerable<DirectoryInfo> WalkParents(DirectoryInfo directory)
    {
        var current = directory;
        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }
}

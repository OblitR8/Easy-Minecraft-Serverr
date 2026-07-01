using System.IO;

namespace Easy_Minecraft_Serverr
{
    // Reads/writes individual keys in a Minecraft server's server.properties file
    // without disturbing any other settings already in there.
    public static class ServerPropertiesService
    {
        private static string GetPropertiesPath(ServerProfile profile) =>
            Path.Combine(profile.InstallPath, "server.properties");

        public static string? GetProperty(ServerProfile profile, string key)
        {
            var path = GetPropertiesPath(profile);
            if (!File.Exists(path)) return null;

            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                    return line.Substring(key.Length + 1);
            }
            return null;
        }

        public static void SetProperty(ServerProfile profile, string key, string value)
        {
            Directory.CreateDirectory(profile.InstallPath);
            var path = GetPropertiesPath(profile);

            List<string> lines = File.Exists(path)
                ? File.ReadAllLines(path).ToList()
                : new List<string>();

            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"{key}={value}";
                    found = true;
                    break;
                }
            }

            if (!found)
                lines.Add($"{key}={value}");

            File.WriteAllLines(path, lines);
        }
    }
}
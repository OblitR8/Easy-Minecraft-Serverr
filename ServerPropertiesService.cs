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

            try
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    if (line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                        return line.Substring(key.Length + 1);
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggingService.LogWarning($"Failed to read property '{key}' from {path}: {ex.Message}");
                return null;
            }
        }

        public static void SetProperty(ServerProfile profile, string key, string value)
        {
            try
            {
                Directory.CreateDirectory(profile.InstallPath);
                var path = GetPropertiesPath(profile);

                // Validate the property
                value = ServerPropertiesValidator.SanitizePropertyValue(value);

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

                // Write to temporary file first, then move (atomic operation)
                string tempPath = path + ".tmp";
                File.WriteAllLines(tempPath, lines);

                // Validate before committing
                var (isValid, errors) = ServerPropertiesValidator.ValidatePropertiesFile(tempPath);
                if (!isValid)
                {
                    File.Delete(tempPath);
                    throw new InvalidOperationException($"Validation failed: {string.Join(", ", errors)}");
                }

                if (File.Exists(path)) File.Delete(path);
                File.Move(tempPath, path);

                LoggingService.LogServerEvent(profile.Name, $"Set property {key}={value}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to set property '{key}' on server '{profile.Name}'", ex);
                throw;
            }
        }
    }
}

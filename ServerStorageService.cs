using System.IO;
using System.Text.Json;

namespace Easy_Minecraft_Serverr
{
    // Plain data shape for saving — keeps JSON decoupled from the live ServerProfile/runtime object
    public class ServerProfileData
    {
        public string Name { get; set; } = "";
        public string InstallPath { get; set; } = "";
        public ServerSoftware Software { get; set; }
        public string MinecraftVersion { get; set; } = "";
        public int RamMinMb { get; set; }
        public int RamMaxMb { get; set; }
        public int Port { get; set; }
        public string JarFileName { get; set; } = "";
    }

    public static class ServerStorageService
    {
        private static readonly string StorageFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasyMinecraftServerr");

        private static readonly string StorageFilePath = Path.Combine(StorageFolder, "servers.json");

        public static void Save(IEnumerable<ServerProfile> servers)
        {
            Directory.CreateDirectory(StorageFolder);

            var data = servers.Select(s => new ServerProfileData
            {
                Name = s.Name,
                InstallPath = s.InstallPath,
                Software = s.Software,
                MinecraftVersion = s.MinecraftVersion,
                RamMinMb = s.RamMinMb,
                RamMaxMb = s.RamMaxMb,
                Port = s.Port,
                JarFileName = s.JarFileName
            }).ToList();

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StorageFilePath, json);
        }

        public static List<ServerProfile> Load()
        {
            if (!File.Exists(StorageFilePath))
                return new List<ServerProfile>();

            try
            {
                var json = File.ReadAllText(StorageFilePath);
                var data = JsonSerializer.Deserialize<List<ServerProfileData>>(json) ?? new();

                return data.Select(d => new ServerProfile
                {
                    Name = d.Name,
                    InstallPath = d.InstallPath,
                    Software = d.Software,
                    MinecraftVersion = d.MinecraftVersion,
                    RamMinMb = d.RamMinMb,
                    RamMaxMb = d.RamMaxMb,
                    Port = d.Port,
                    JarFileName = string.IsNullOrEmpty(d.JarFileName) ? "server.jar" : d.JarFileName
                }).ToList();
            }
            catch
            {
                // Corrupted file — don't crash the app, just start fresh
                return new List<ServerProfile>();
            }
        }
    }
}
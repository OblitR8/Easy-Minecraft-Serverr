using System.IO;

namespace Easy_Minecraft_Serverr
{
    public static class ModManagerService
    {
        public static bool SupportsAddons(ServerSoftware software) =>
            software == ServerSoftware.Paper || software == ServerSoftware.Fabric;

        public static string GetAddonFolderName(ServerSoftware software) => software switch
        {
            ServerSoftware.Paper => "plugins",
            ServerSoftware.Fabric => "mods",
            _ => ""
        };

        public static string GetAddonLabel(ServerSoftware software) => software switch
        {
            ServerSoftware.Paper => "Plugins",
            ServerSoftware.Fabric => "Mods",
            _ => "Add-ons"
        };

        public static string GetAddonFolderPath(ServerProfile profile) =>
            Path.Combine(profile.InstallPath, GetAddonFolderName(profile.Software));

        public static List<string> ListInstalled(ServerProfile profile)
        {
            var folder = GetAddonFolderPath(profile);
            if (!SupportsAddons(profile.Software) || !Directory.Exists(folder))
                return new List<string>();

            return Directory.GetFiles(folder, "*.jar")
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .ToList()!;
        }

        public static void AddAddon(ServerProfile profile, string sourceFilePath)
        {
            var folder = GetAddonFolderPath(profile);
            Directory.CreateDirectory(folder);
            var dest = Path.Combine(folder, Path.GetFileName(sourceFilePath));
            File.Copy(sourceFilePath, dest, overwrite: true);
        }

        public static void RemoveAddon(ServerProfile profile, string fileName)
        {
            var path = Path.Combine(GetAddonFolderPath(profile), fileName);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
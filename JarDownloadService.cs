using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Easy_Minecraft_Serverr
{
    public static class JarDownloadService
    {
        private static readonly HttpClient _http = CreateClient();

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EasyMinecraftServerr/1.0 (https://github.com/yourname/easy-minecraft-serverr)");
            client.Timeout = TimeSpan.FromMinutes(5);
            return client;
        }

        public static async Task DownloadServerJarAsync(ServerProfile profile, IProgress<string>? statusProgress = null)
        {
            Directory.CreateDirectory(profile.InstallPath);
            string jarPath = Path.Combine(profile.InstallPath, profile.JarFileName);

            statusProgress?.Report($"Resolving download URL for {profile.Software} {profile.MinecraftVersion}...");

            string downloadUrl = profile.Software switch
            {
                ServerSoftware.Vanilla => await GetVanillaDownloadUrlAsync(profile.MinecraftVersion),
                ServerSoftware.Paper => await GetPaperDownloadUrlAsync(profile.MinecraftVersion),
                ServerSoftware.Fabric => await GetFabricDownloadUrlAsync(profile.MinecraftVersion),
                _ => throw new NotSupportedException($"{profile.Software} auto-download isn't supported yet. Install it manually into: {profile.InstallPath}")
            };

            statusProgress?.Report("Downloading server jar...");
            using var response = await _http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var fileStream = File.Create(jarPath);
            await response.Content.CopyToAsync(fileStream);

            statusProgress?.Report("Download complete.");
        }

        private static async Task<string> GetVanillaDownloadUrlAsync(string version)
        {
            var manifestJson = await _http.GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            using var manifestDoc = JsonDocument.Parse(manifestJson);

            string? versionUrl = null;
            foreach (var v in manifestDoc.RootElement.GetProperty("versions").EnumerateArray())
            {
                if (v.GetProperty("id").GetString() == version)
                {
                    versionUrl = v.GetProperty("url").GetString();
                    break;
                }
            }
            if (versionUrl == null)
                throw new Exception($"Vanilla version {version} not found in manifest.");

            var versionJson = await _http.GetStringAsync(versionUrl);
            using var versionDoc = JsonDocument.Parse(versionJson);
            return versionDoc.RootElement
                .GetProperty("downloads")
                .GetProperty("server")
                .GetProperty("url")
                .GetString()!;
        }

        private static async Task<string> GetPaperDownloadUrlAsync(string version)
        {
            var buildsJson = await _http.GetStringAsync($"https://fill.papermc.io/v3/projects/paper/versions/{version}/builds");
            using var doc = JsonDocument.Parse(buildsJson);

            JsonElement? latestStable = null;
            foreach (var build in doc.RootElement.EnumerateArray())
            {
                if (build.GetProperty("channel").GetString() == "STABLE")
                    latestStable = build;
            }

            if (latestStable == null)
                throw new Exception($"No stable Paper build found for {version}.");

            return latestStable.Value
                .GetProperty("downloads")
                .GetProperty("server:default")
                .GetProperty("url")
                .GetString()!;
        }

        private static async Task<string> GetFabricDownloadUrlAsync(string version)
        {
            var loadersJson = await _http.GetStringAsync($"https://meta.fabricmc.net/v2/versions/loader/{version}");
            using var loadersDoc = JsonDocument.Parse(loadersJson);
            var loaderArray = loadersDoc.RootElement.EnumerateArray();
            if (!loaderArray.MoveNext())
                throw new Exception($"No Fabric loader available for Minecraft {version}.");
            string loaderVersion = loaderArray.Current.GetProperty("loader").GetProperty("version").GetString()!;

            var installersJson = await _http.GetStringAsync("https://meta.fabricmc.net/v2/versions/installer");
            using var installersDoc = JsonDocument.Parse(installersJson);
            string installerVersion = installersDoc.RootElement[0].GetProperty("version").GetString()!;

            return $"https://meta.fabricmc.net/v2/versions/loader/{version}/{loaderVersion}/{installerVersion}/server/jar";
        }
    }
}
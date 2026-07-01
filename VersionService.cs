using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Easy_Minecraft_Serverr
{
    public static class VersionService
    {
        private static readonly HttpClient _http = CreateClient();

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EasyMinecraftServerr/1.0 (https://github.com/yourname/easy-minecraft-serverr)");
            return client;
        }

        public static async Task<List<string>> GetVersionsAsync(ServerSoftware software)
        {
            return software switch
            {
                ServerSoftware.Vanilla => await GetVanillaVersionsAsync(),
                ServerSoftware.Paper => await GetPaperVersionsAsync(),
                ServerSoftware.Fabric => await GetFabricVersionsAsync(),
                _ => new List<string>()
            };
        }

        private static async Task<List<string>> GetVanillaVersionsAsync()
        {
            var json = await _http.GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            using var doc = JsonDocument.Parse(json);
            var versions = new List<string>();
            foreach (var v in doc.RootElement.GetProperty("versions").EnumerateArray())
            {
                if (v.GetProperty("type").GetString() == "release")
                    versions.Add(v.GetProperty("id").GetString()!);
            }
            return versions;
        }

        private static async Task<List<string>> GetPaperVersionsAsync()
        {
            var json = await _http.GetStringAsync("https://fill.papermc.io/v3/projects/paper");
            using var doc = JsonDocument.Parse(json);

            var versions = new List<string>();
            foreach (var group in doc.RootElement.GetProperty("versions").EnumerateObject())
            {
                foreach (var v in group.Value.EnumerateArray())
                    versions.Add(v.GetString()!);
            }

            versions.Reverse();
            return versions;
        }

        private static async Task<List<string>> GetFabricVersionsAsync()
        {
            var json = await _http.GetStringAsync("https://meta.fabricmc.net/v2/versions/game");
            using var doc = JsonDocument.Parse(json);
            var versions = new List<string>();
            foreach (var v in doc.RootElement.EnumerateArray())
            {
                if (v.GetProperty("stable").GetBoolean())
                    versions.Add(v.GetProperty("version").GetString()!);
            }
            return versions;
        }
    }
}
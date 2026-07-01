using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Easy_Minecraft_Serverr
{
    public class UpdateInfo
    {
        public bool UpdateAvailable { get; set; }
        public string LatestVersion { get; set; } = "";
        public string CurrentVersion { get; set; } = "";
        public string ReleaseUrl { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
    }

    public static class UpdateCheckService
    {
        // TODO: replace with your actual GitHub username/repo, e.g. "johnsmith/easy-minecraft-serverr"
        private const string GitHubOwnerRepo = "OblitR8/Easy-Minecraft-Serverr";

        // Bump this constant with every release you publish
        public const string CurrentVersion = "1.1";

        private static readonly HttpClient _http = CreateClient();

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            // GitHub API requires a User-Agent header or it rejects the request
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EasyMinecraftServerr-UpdateChecker/1.0");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return client;
        }

        public static async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            try
            {
                var json = await _http.GetStringAsync($"https://api.github.com/repos/{GitHubOwnerRepo}/releases/latest");
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string tagName = root.GetProperty("tag_name").GetString() ?? "";
                string latestVersion = tagName.TrimStart('v', 'V');
                string releaseUrl = root.GetProperty("html_url").GetString() ?? "";
                string releaseNotes = root.TryGetProperty("body", out var bodyProp) ? (bodyProp.GetString() ?? "") : "";

                bool isNewer = IsVersionNewer(latestVersion, CurrentVersion);

                return new UpdateInfo
                {
                    UpdateAvailable = isNewer,
                    LatestVersion = latestVersion,
                    CurrentVersion = CurrentVersion,
                    ReleaseUrl = releaseUrl,
                    ReleaseNotes = releaseNotes
                };
            }
            catch
            {
                // No internet, repo not found, rate-limited, etc. — fail silently, don't block app startup
                return null;
            }
        }

        private static bool IsVersionNewer(string latest, string current)
        {
            if (Version.TryParse(latest, out var latestVer) && Version.TryParse(current, out var currentVer))
                return latestVer > currentVer;

            // Fallback: not parseable as System.Version, just compare as plain strings
            return string.Compare(latest, current, StringComparison.OrdinalIgnoreCase) > 0;
        }
    }
}
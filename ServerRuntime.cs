using System.Diagnostics;
using System.IO;

namespace Easy_Minecraft_Serverr
{
    public class ServerRuntime
    {
        private Process? _process;
        public bool IsRunning => _process != null && !_process.HasExited;

        public event Action<string>? OutputReceived;
        public event Action? Exited;

        public async Task StartAsync(ServerProfile profile)
        {
            if (IsRunning) return;

            Directory.CreateDirectory(profile.InstallPath);
            string jarPath = Path.Combine(profile.InstallPath, profile.JarFileName);

            if (!File.Exists(jarPath))
            {
                OutputReceived?.Invoke("[EasyMC] Server jar not found — downloading...");
                var progress = new Progress<string>(msg => OutputReceived?.Invoke($"[EasyMC] {msg}"));
                try
                {
                    await JarDownloadService.DownloadServerJarAsync(profile, progress);
                }
                catch (Exception ex)
                {
                    OutputReceived?.Invoke($"[EasyMC] Download failed: {ex.Message}");
                    return;
                }
            }

            string eulaPath = Path.Combine(profile.InstallPath, "eula.txt");
            await File.WriteAllTextAsync(eulaPath, "eula=true\n");

            var startInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-Xms{profile.RamMinMb}M -Xmx{profile.RamMaxMb}M -jar \"{profile.JarFileName}\" nogui",
                WorkingDirectory = profile.InstallPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            _process.OutputDataReceived += (s, e) => { if (e.Data != null) OutputReceived?.Invoke(e.Data); };
            _process.ErrorDataReceived += (s, e) => { if (e.Data != null) OutputReceived?.Invoke(e.Data); };
            _process.Exited += (s, e) => Exited?.Invoke();

            try
            {
                _process.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                OutputReceived?.Invoke("[EasyMC] ERROR: Couldn't launch Java. Make sure Java (JDK/JRE) is installed and added to your PATH.");
                _process = null;
                return;
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public async Task SendCommandAsync(string command)
        {
            if (!IsRunning || _process!.StandardInput == null) return;
            await _process.StandardInput.WriteLineAsync(command);
            await _process.StandardInput.FlushAsync();
        }

        public async Task StopAsync()
        {
            if (!IsRunning) return;
            await SendCommandAsync("stop");

            for (int i = 0; i < 20 && IsRunning; i++)
                await Task.Delay(500);

            if (IsRunning)
                _process!.Kill(entireProcessTree: true);
        }
    }
}
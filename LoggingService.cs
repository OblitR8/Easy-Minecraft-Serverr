using System;
using System.IO;
using System.Text;

namespace Easy_Minecraft_Serverr
{
    public static class LoggingService
    {
        private static readonly string LogFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasyMinecraftServerr", "logs");

        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            lock (_lockObject)
            {
                if (_initialized) return;
                try
                {
                    Directory.CreateDirectory(LogFolder);
                    _initialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize logging: {ex.Message}");
                }
            }
        }

        public static void LogError(string message, Exception? ex = null)
        {
            LogMessage("ERROR", message, ex);
        }

        public static void LogWarning(string message)
        {
            LogMessage("WARN", message);
        }

        public static void LogInfo(string message)
        {
            LogMessage("INFO", message);
        }

        public static void LogServerEvent(string serverName, string message)
        {
            if (!_initialized) Initialize();

            string fileName = Path.Combine(LogFolder, $"server-{SanitizeFileName(serverName)}.log");
            WriteToFile(fileName, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }

        private static void LogMessage(string level, string message, Exception? ex = null)
        {
            if (!_initialized) Initialize();

            string fileName = Path.Combine(LogFolder, "application.log");
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");

            if (ex != null)
            {
                sb.AppendLine($"Exception: {ex.GetType().Name}");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
            }

            WriteToFile(fileName, sb.ToString());
        }

        private static void WriteToFile(string filePath, string content)
        {
            lock (_lockObject)
            {
                try
                {
                    // Keep log files to 10MB max, rotate if needed
                    if (File.Exists(filePath) && new FileInfo(filePath).Length > 10 * 1024 * 1024)
                    {
                        string backupPath = filePath + ".old";
                        if (File.Exists(backupPath)) File.Delete(backupPath);
                        File.Move(filePath, backupPath);
                    }

                    File.AppendAllText(filePath, content + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}");
                }
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}

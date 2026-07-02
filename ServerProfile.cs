using System.ComponentModel;
using System.Text;

namespace Easy_Minecraft_Serverr
{
    public enum ServerSoftware
    {
        Vanilla,
        Paper,
        Fabric
    }

    public enum ServerStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping
    }

    public class ServerProfile : INotifyPropertyChanged
    {
        private string _name = "New Server";
        private ServerSoftware _software = ServerSoftware.Paper;
        private string _minecraftVersion = "1.21";
        private ServerStatus _status = ServerStatus.Stopped;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string InstallPath { get; set; } = "";

        public ServerSoftware Software
        {
            get => _software;
            set
            {
                _software = value;
                OnPropertyChanged(nameof(Software));
                OnPropertyChanged(nameof(SoftwareVersionLabel));
            }
        }

        public string MinecraftVersion
        {
            get => _minecraftVersion;
            set
            {
                _minecraftVersion = value;
                OnPropertyChanged(nameof(MinecraftVersion));
                OnPropertyChanged(nameof(SoftwareVersionLabel));
            }
        }

        public string SoftwareVersionLabel => $"{Software} • {MinecraftVersion}";

        public int RamMinMb { get; set; } = 1024;
        public int RamMaxMb { get; set; } = 4096;
        public int Port { get; set; } = 25565;
        public string JarFileName { get; set; } = "server.jar";

        public ServerRuntime Runtime { get; }
        public ServerPerformanceMonitor PerformanceMonitor { get; }
        public OperationState OperationState { get; }
        public StringBuilder ConsoleLog { get; } = new StringBuilder();

        public event Action<string>? ConsoleLineAdded;

        public ServerProfile()
        {
            Runtime = new ServerRuntime();
            PerformanceMonitor = new ServerPerformanceMonitor();
            OperationState = new OperationState();

            Runtime.OutputReceived += line =>
            {
                ConsoleLog.AppendLine(line);
                ConsoleLineAdded?.Invoke(line);
            };
            Runtime.Exited += () =>
            {
                Status = ServerStatus.Stopped;
                PerformanceMonitor.StopMonitoring();
                OperationState.CompleteOperation();
                LoggingService.LogServerEvent(Name, "Server stopped");
            };
        }

        public ServerStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public override string ToString() => Name;
    }
}

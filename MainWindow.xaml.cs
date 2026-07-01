using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Easy_Minecraft_Serverr
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<ServerProfile> Servers { get; } = new();

        private bool _isLoadingProfile = false;
        private ServerProfile? _consoleSubscribedServer;

        private ServerProfile? _selectedServer;
        public ServerProfile? SelectedServer
        {
            get => _selectedServer;
            set
            {
                _selectedServer = value;
                OnPropertyChanged(nameof(SelectedServer));
                OnPropertyChanged(nameof(IsServerSelected));
                LoadProfileIntoUi();
            }
        }

        public bool IsServerSelected => SelectedServer != null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SoftwareCombo.ItemsSource = Enum.GetValues(typeof(ServerSoftware));

            foreach (var server in ServerStorageService.Load())
                Servers.Add(server);

            if (Servers.Count > 0)
                SelectedServer = Servers[0];

            Closing += MainWindow_Closing;

            Title = $"Easy Minecraft Serverr v{UpdateCheckService.CurrentVersion}";
            _ = CheckForUpdatesOnStartupAsync();
        }

        private async Task CheckForUpdatesOnStartupAsync()
        {
            var update = await UpdateCheckService.CheckForUpdateAsync();
            if (update == null || !update.UpdateAvailable) return;

            var result = MessageBox.Show(
                $"A new version is available!\n\n" +
                $"Current version: {update.CurrentVersion}\n" +
                $"Latest version: {update.LatestVersion}\n\n" +
                $"Would you like to open the download page now?",
                "Update Available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(update.ReleaseUrl))
            {
                Process.Start(new ProcessStartInfo { FileName = update.ReleaseUrl, UseShellExecute = true });
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            ServerStorageService.Save(Servers);
        }

        private void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            var name = $"Server {Servers.Count + 1}";
            var installPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyMinecraftServerr", name);

            var newServer = new ServerProfile
            {
                Name = name,
                InstallPath = installPath
            };
            Servers.Add(newServer);
            SelectedServer = newServer;
            ServerStorageService.Save(Servers);
        }

        private void RemoveServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;
            Servers.Remove(SelectedServer);
            SelectedServer = Servers.Count > 0 ? Servers[0] : null;
            ServerStorageService.Save(Servers);
        }

        private void LoadProfileIntoUi()
        {
            if (_consoleSubscribedServer != null)
                _consoleSubscribedServer.ConsoleLineAdded -= OnConsoleLineAdded;

            if (SelectedServer == null)
            {
                ConsoleOutput.Text = "";
                RefreshAddonTab();
                return;
            }

            _isLoadingProfile = true;

            SoftwareCombo.SelectedItem = SelectedServer.Software; // triggers SoftwareCombo_SelectionChanged
            RamMinSlider.Value = SelectedServer.RamMinMb;
            RamMaxSlider.Value = SelectedServer.RamMaxMb;
            PortBox.Text = SelectedServer.Port.ToString();
            PortStatusText.Text = "";

            ConsoleOutput.Text = SelectedServer.ConsoleLog.ToString();
            ConsoleOutput.ScrollToEnd();
            StartStopButton.Content = SelectedServer.Runtime.IsRunning ? "Stop" : "Start";

            _consoleSubscribedServer = SelectedServer;
            SelectedServer.ConsoleLineAdded += OnConsoleLineAdded;

            RefreshAddonTab();

            _isLoadingProfile = false;
        }

        private void OnConsoleLineAdded(string line)
        {
            Dispatcher.Invoke(() =>
            {
                ConsoleOutput.AppendText(line + Environment.NewLine);
                ConsoleOutput.ScrollToEnd();
            });
        }

        private async void SoftwareCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoftwareCombo.SelectedItem is not ServerSoftware software) return;

            bool loadingProfile = _isLoadingProfile;
            string? preferredVersion = loadingProfile ? SelectedServer?.MinecraftVersion : null;

            if (!loadingProfile && SelectedServer != null)
            {
                SelectedServer.Software = software;
                ServerStorageService.Save(Servers);
            }

            await PopulateVersionsAsync(software, preferredVersion);
            RefreshAddonTab();
        }

        private async Task PopulateVersionsAsync(ServerSoftware software, string? preferredVersion = null)
        {
            VersionCombo.ItemsSource = null;
            VersionStatusText.Text = "Loading versions...";
            try
            {
                var versions = await VersionService.GetVersionsAsync(software);
                VersionCombo.ItemsSource = versions;

                if (preferredVersion != null && versions.Contains(preferredVersion))
                    VersionCombo.SelectedItem = preferredVersion;
                else if (versions.Count > 0)
                    VersionCombo.SelectedIndex = 0;

                VersionStatusText.Text = "";
            }
            catch (Exception ex)
            {
                VersionStatusText.Text = $"Failed to load versions: {ex.Message}";
            }
        }

        private void VersionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingProfile) return;
            if (SelectedServer != null && VersionCombo.SelectedItem is string version)
            {
                SelectedServer.MinecraftVersion = version;
                ServerStorageService.Save(Servers);
            }
        }

        private void RamMinSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoadingProfile || SelectedServer == null) return;
            SelectedServer.RamMinMb = (int)RamMinSlider.Value;
            ServerStorageService.Save(Servers);
        }

        private void RamMaxSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoadingProfile || SelectedServer == null) return;
            SelectedServer.RamMaxMb = (int)RamMaxSlider.Value;
            ServerStorageService.Save(Servers);
        }

        private void SavePortButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;

            if (!int.TryParse(PortBox.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                PortStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                PortStatusText.Text = "Invalid port. Enter a number between 1 and 65535.";
                return;
            }

            SelectedServer.Port = port;
            ServerStorageService.Save(Servers);
            ServerPropertiesService.SetProperty(SelectedServer, "server-port", port.ToString());

            PortStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            PortStatusText.Text = SelectedServer.Runtime.IsRunning
                ? $"Saved. Restart the server for port {port} to take effect."
                : $"Saved. Port {port} will be used next time this server starts.";
        }

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;
            var server = SelectedServer;

            if (server.Runtime.IsRunning)
            {
                StartStopButton.IsEnabled = false;
                server.Status = ServerStatus.Stopping;
                await server.Runtime.StopAsync();
                server.Status = ServerStatus.Stopped;
                StartStopButton.Content = "Start";
                StartStopButton.IsEnabled = true;
            }
            else
            {
                StartStopButton.IsEnabled = false;
                server.Status = ServerStatus.Starting;
                await server.Runtime.StartAsync(server);
                server.Status = server.Runtime.IsRunning ? ServerStatus.Running : ServerStatus.Stopped;
                StartStopButton.Content = server.Runtime.IsRunning ? "Stop" : "Start";
                StartStopButton.IsEnabled = true;
            }
        }

        private async void SendCommandButton_Click(object sender, RoutedEventArgs e)
        {
            await SendCurrentCommand();
        }

        private async void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await SendCurrentCommand();
        }

        private async Task SendCurrentCommand()
        {
            if (SelectedServer == null) return;
            string command = CommandInput.Text.Trim();
            if (string.IsNullOrEmpty(command)) return;

            await SelectedServer.Runtime.SendCommandAsync(command);
            CommandInput.Clear();
        }

        private void RefreshAddonTab()
        {
            if (SelectedServer == null)
            {
                PluginsModsTab.IsEnabled = false;
                AddonListBox.ItemsSource = null;
                return;
            }

            bool supported = ModManagerService.SupportsAddons(SelectedServer.Software);
            PluginsModsTab.IsEnabled = supported;

            if (!supported)
            {
                AddonHeaderText.Text = $"{SelectedServer.Software} doesn't support plugins/mods through this manager.";
                AddonListBox.ItemsSource = null;
                return;
            }

            string label = ModManagerService.GetAddonLabel(SelectedServer.Software);
            var installed = ModManagerService.ListInstalled(SelectedServer);
            AddonHeaderText.Text = $"Installed {label} ({installed.Count})";
            AddonListBox.ItemsSource = installed;
        }

        private void AddAddonButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null || !ModManagerService.SupportsAddons(SelectedServer.Software)) return;

            string label = ModManagerService.GetAddonLabel(SelectedServer.Software).TrimEnd('s');

            var dialog = new OpenFileDialog
            {
                Title = $"Select a {label} .jar file",
                Filter = "Jar files (*.jar)|*.jar",
                Multiselect = true
            };

            if (dialog.ShowDialog() != true) return;

            var warning = MessageBox.Show(
                $"You're about to install {dialog.FileNames.Length} file(s) into this server's {ModManagerService.GetAddonFolderName(SelectedServer.Software)} folder.\n\n" +
                "Only install jars from sources you trust — plugins and mods run with full access to your server and can contain malicious code.\n\n" +
                "A server restart is required for changes to take effect. Continue?",
                "Confirm installation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (warning != MessageBoxResult.Yes) return;

            foreach (var file in dialog.FileNames)
                ModManagerService.AddAddon(SelectedServer, file);

            RefreshAddonTab();
        }

        private void RemoveAddonButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;
            if (AddonListBox.SelectedItem is not string fileName) return;

            var warning = MessageBox.Show(
                $"Remove \"{fileName}\" from this server?\n\nThis deletes the file permanently and requires a server restart to take effect.",
                "Confirm removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (warning != MessageBoxResult.Yes) return;

            ModManagerService.RemoveAddon(SelectedServer, fileName);
            RefreshAddonTab();
        }

        private void OpenAddonFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;
            var folder = ModManagerService.GetAddonFolderPath(SelectedServer);
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Easy_Minecraft_Serverr
{
    public class ServerPerformanceMonitor : INotifyPropertyChanged, IDisposable
    {
        private Process? _process;
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _ramCounter;
        private DateTime _startTime;
        private CancellationTokenSource? _cancellationTokenSource;

        private double _cpuUsage = 0;
        private long _memoryUsageMb = 0;
        private string _uptime = "00:00:00";
        private bool _isMonitoring = false;

        public double CpuUsage
        {
            get => _cpuUsage;
            private set
            {
                if (_cpuUsage != value)
                {
                    _cpuUsage = value;
                    OnPropertyChanged(nameof(CpuUsage));
                }
            }
        }

        public long MemoryUsageMb
        {
            get => _memoryUsageMb;
            private set
            {
                if (_memoryUsageMb != value)
                {
                    _memoryUsageMb = value;
                    OnPropertyChanged(nameof(MemoryUsageMb));
                }
            }
        }

        public string Uptime
        {
            get => _uptime;
            private set
            {
                if (_uptime != value)
                {
                    _uptime = value;
                    OnPropertyChanged(nameof(Uptime));
                }
            }
        }

        public bool IsMonitoring => _isMonitoring;

        public void StartMonitoring(Process process, ServerProfile profile)
        {
            if (_isMonitoring) StopMonitoring();

            _process = process;
            _startTime = DateTime.Now;
            _cancellationTokenSource = new CancellationTokenSource();
            _isMonitoring = true;

            try
            {
                _cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                _ramCounter = new PerformanceCounter("Process", "Working Set", process.ProcessName);
                _cpuCounter.NextValue(); // Initialize
            }
            catch (Exception ex)
            {
                LoggingService.LogWarning($"Could not initialize performance counters: {ex.Message}");
            }

            _ = MonitoringLoopAsync(_cancellationTokenSource.Token);
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            _cancellationTokenSource?.Cancel();
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
            _cpuCounter = null;
            _ramCounter = null;
            CpuUsage = 0;
            MemoryUsageMb = 0;
            Uptime = "00:00:00";
        }

        private async Task MonitoringLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isMonitoring)
            {
                try
                {
                    if (_process != null && !_process.HasExited)
                    {
                        // Update CPU usage
                        if (_cpuCounter != null)
                        {
                            float rawCpu = _cpuCounter.NextValue();
                            CpuUsage = Math.Round(rawCpu / Environment.ProcessorCount, 2);
                        }

                        // Update memory usage
                        if (_ramCounter != null)
                        {
                            long memoryBytes = (long)_ramCounter.NextValue();
                            MemoryUsageMb = memoryBytes / (1024 * 1024);
                        }

                        // Update uptime
                        TimeSpan elapsed = DateTime.Now - _startTime;
                        Uptime = elapsed.ToString(@"hh\:mm\:ss");
                    }
                    else
                    {
                        StopMonitoring();
                        break;
                    }

                    await Task.Delay(1000, cancellationToken); // Update every second
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LoggingService.LogWarning($"Error in performance monitoring: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            StopMonitoring();
            _cancellationTokenSource?.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

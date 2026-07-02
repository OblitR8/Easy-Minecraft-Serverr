using System.Windows;

namespace Easy_Minecraft_Serverr
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize logging service
            LoggingService.Initialize();
            LoggingService.LogInfo("Easy Minecraft Serverr started");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggingService.LogInfo("Easy Minecraft Serverr closed");
            base.OnExit(e);
        }
    }
}

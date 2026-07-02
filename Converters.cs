using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Easy_Minecraft_Serverr
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerStatus status)
            {
                return status switch
                {
                    ServerStatus.Running => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34D399")),
                    ServerStatus.Starting => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24")),
                    ServerStatus.Stopping => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB923C")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerStatus status)
            {
                return status switch
                {
                    ServerStatus.Running => "Running",
                    ServerStatus.Starting => "Starting",
                    ServerStatus.Stopping => "Stopping",
                    ServerStatus.Stopped => "Stopped",
                    _ => "Unknown"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Easy_Minecraft_Serverr
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ServerStatus status) return Brushes.Gray;

            return status switch
            {
                ServerStatus.Running => new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)),   // emerald
                ServerStatus.Starting => new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)),  // amber
                ServerStatus.Stopping => new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)),  // amber
                ServerStatus.Stopped => new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),   // muted gray
                _ => Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class StatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ServerStatus status) return "Unknown";

            return status switch
            {
                ServerStatus.Running => "Running",
                ServerStatus.Starting => "Starting…",
                ServerStatus.Stopping => "Stopping…",
                ServerStatus.Stopped => "Stopped",
                _ => "Unknown"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
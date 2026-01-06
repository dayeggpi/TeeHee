using System.Globalization;
using System.Windows.Data;

namespace TeeHee;

public class TruncateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var firstLine = lines.FirstOrDefault() ?? text;
            var hasMoreLines = lines.Length > 1;
            
            if (firstLine.Length > 56)
            {
                return firstLine.Substring(0, 56) + "...";
            }
            
            if (hasMoreLines)
            {
                return firstLine + "...";
            }
            
            return firstLine;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
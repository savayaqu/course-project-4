using Humanizer;
using System.Globalization;

namespace PicsyncClient.Converters;

public class DateTimePositionConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not DateTime dateTime) return value ?? "";

        return dateTime.Humanize();
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

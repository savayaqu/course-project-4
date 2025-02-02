using PicsyncClient.Utils;
using System.Globalization;

namespace PicsyncClient.Converters;

public class ToPercentConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (!Helpers.SafeGetDouble(value, out double num)) return "Ошибка";

        return Math.Round(num * 100) + "%";
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

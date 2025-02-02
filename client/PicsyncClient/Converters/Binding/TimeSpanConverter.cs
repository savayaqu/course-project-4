using PicsyncClient.Utils;
using System.Globalization;

namespace PicsyncClient.Converters;

public class TimeSpanConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not TimeSpan timeSpan) return "Ошибка";

        if (timeSpan.Hours >= 1)
            return $"{timeSpan.Hours} часов, {timeSpan.Minutes} минут";
        else if (timeSpan.Minutes >= 1)
            return $"{timeSpan.Minutes} минут, {timeSpan.Seconds} секунд";
        else
            return $"{timeSpan.Seconds} секунд";
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

using PicsyncClient.Utils;
using System.Globalization;

namespace PicsyncClient.Converters;

public class TimeSpanConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not TimeSpan timeSpan) return "Ошибка";
        
        if (timeSpan.Days >= 1)
            return $"{timeSpan.Days} сут, {timeSpan.Hours} час";

        else if(timeSpan.Hours >= 1)
            return $"{timeSpan.Hours} час, {timeSpan.Minutes} мин";

        else if (timeSpan.Minutes >= 1)
            return $"{timeSpan.Minutes} мин, {timeSpan.Seconds} сек";

        else
            return $"{timeSpan.Seconds} сек";
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

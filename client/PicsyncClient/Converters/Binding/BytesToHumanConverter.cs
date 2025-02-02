using PicsyncClient.Utils;
using System.Globalization;

namespace PicsyncClient.Converters;

public class BytesToHumanConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not ulong bytes) return "Ошибка";
        return Helpers.BytesToHuman(bytes);
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

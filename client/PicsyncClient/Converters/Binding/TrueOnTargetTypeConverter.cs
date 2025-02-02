using PicsyncClient.Utils;
using System.Globalization;

namespace PicsyncClient.Converters;

public class TrueOnValueOfTypeConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        Type? type = parameter as Type;

        if (type == null || value == null) return false;

        return type.IsAssignableFrom(value.GetType());
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

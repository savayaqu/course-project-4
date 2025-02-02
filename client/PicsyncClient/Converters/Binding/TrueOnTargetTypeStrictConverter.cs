using System.Diagnostics;
using System.Globalization;

namespace PicsyncClient.Converters;

public class TrueOnTargetTypeStrictConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        Type? type = parameter as Type;

        if (type == null || value == null) return false;

        return value.GetType() == type;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

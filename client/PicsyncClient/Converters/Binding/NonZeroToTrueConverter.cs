using System.Globalization;

namespace PicsyncClient.Converters;

public class NonZeroToTrueConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value == null)
            return false;

        try
        {
            double doubleValue = System.Convert.ToDouble(value);
            return doubleValue != 0;
        }
        catch
        {
            return false;
        }
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

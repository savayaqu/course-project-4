using System.Diagnostics;
using System.Globalization;
using PicsyncClient.Utils;

namespace PicsyncClient.Converters;

public class CountSuffixConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            ulong num = System.Convert.ToUInt64(value);
            return Helpers.CountToHuman(num);
        }
        catch
        {
            return value ?? "";
        }
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
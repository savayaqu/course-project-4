using System.Diagnostics;
using System.Globalization;
using static System.Math;

namespace PicsyncClient.Converters;

public class CountSuffixConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            ulong num = System.Convert.ToUInt64(value);
            return FormatNumber(num);
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

    public readonly string[] Suffixes = ["", "K", "M", "B", "T", "Q", "Qt", "Sx"];

    public string FormatNumber(ulong number)
    {
        int pow = (int)Floor((number > 0 ? Log(number) : 0) / Log(1000));
        pow = Min(pow, Suffixes.Length - 1);

        double value = number / Pow(1000, pow);

        string formatted = value % 1 == 0 
            ? value.ToString("F0")
            : value.ToString("F" + 
                (3 - Floor(Log10(value) + 1)).ToString()
            );

        return formatted + Suffixes[pow];
    }
}
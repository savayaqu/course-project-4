using System.Globalization;

namespace PicsyncClient.Converters;

public class ErrorConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is Dictionary<string, List<string>> errors 
            && parameter is string key 
            && errors.ContainsKey(key)
        ) {
            return string.Join("\n", errors[key]);
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

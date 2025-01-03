using System.Diagnostics;
using System.Globalization;

namespace PicsyncClient.Converters;

public class LocalUriConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is string uri) 
        {
            Debug.WriteLine("LocalUri: " + uri);
            uri = uri.Replace("file:/", "");
            uri = uri.Replace("/storage/emulated/0/", "");
            return uri;
        }
        Debug.WriteLine("LocalUri: NOT A STRING");
        return value;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

using System.Globalization;

namespace PicsyncClient.Converters;

public class FalseOnUriRoot : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is string uri) 
        {
            uri = uri.Replace("file:/", "");
            uri = uri.Replace("/storage/emulated/0/", "");
            return uri.IndexOf('/') != -1;
        }
        return true;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

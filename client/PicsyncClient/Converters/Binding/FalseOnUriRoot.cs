using PicsyncClient.Models.Albums;
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
            return uri.Contains('/');
        }
        if (value is IAlbumLocal album)
        {
            string path = album.LocalPath.Replace("file:/", "");
            path = path.Replace("/storage/emulated/0/", "");
            return !(album.Name == path);
        }
        return true;
    }

    public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

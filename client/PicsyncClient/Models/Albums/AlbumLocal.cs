using PicsyncClient.Models.Pictures;
using SQLite;

namespace PicsyncClient.Models.Albums;

public class AlbumLocal : AlbumBase
{
    // Свойства
    [Ignore] public int?      NameDuplicaIndex { get; set; } = null;
    public List<PictureLocal> LocalPictures    { get; set; } = [];
    public required string    LocalPath        { get; set; }

    // Геттеры
    public override int PicturesCount => LocalPictures.Count;

    private List<string>? _thumbnailPaths = null;
    public override List<string> ThumbnailPaths
    {
        get
        {
            if (_thumbnailPaths != null) 
                return _thumbnailPaths;

            _thumbnailPaths = [];
            byte i = 0;
            foreach (var picture in LocalPictures)
            {
                if (i++ > 2) break; // TODO: FIXME: 2 -> 4

                if (picture.LocalPath is string path)
                    _thumbnailPaths.Add(path);
            }
            return _thumbnailPaths;
        }
    }
}

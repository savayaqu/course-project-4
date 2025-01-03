using PicsyncClient.Models.Pictures;
using SQLite;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Albums;

public class AlbumSynced : AlbumRemote, IAlbumLocal
{
    // Свойства
    [JsonPropertyName("name")]
    string IAlbum.Name
    { 
        get {
            return base.Name;
        }
        set
        {
            ((IAlbum)this).Name = value;
        }
    }
    [Ignore] public int?       NameDuplicaIndex { get; set; } = null;
    public List<PictureLocal>  LocalPictures    { get; set; } = [];
    public required string     LocalPath        { get; set; }

    // Геттеры
    public int TotalPicturesCount => (LocalPictures?.Count ?? 0) + RemotePicturesCount; // TODO: бред

    private List<string>? _thumbnailPaths = null;
    public new List<string> ThumbnailPaths
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


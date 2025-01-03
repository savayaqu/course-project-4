using PicsyncClient.Models.Albums;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public class PictureLocal : PictureBase
{
    // Свойства
    public override string Name => Path.GetFileName(LocalPath);
    public required string LocalPath { get; set; }

    [JsonIgnore]
    public AlbumLocal SpecificAlbum
    {
        get => (AlbumLocal)Album;
        set => Album = value;
    }

    // Геттеры
    public override string OriginalPath  => LocalPath;
    public override string ThumbnailPath => LocalPath;
}
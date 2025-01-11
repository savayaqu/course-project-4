using PicsyncClient.Models.Albums;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public interface IPictureLocal : IPicture
{
    // Свойства
    public string LocalPath { get; set; }

    [JsonIgnore]
    public AlbumLocal SpecificAlbum
    {
        get => (AlbumLocal)Album;
        set => Album = value;
    }
}
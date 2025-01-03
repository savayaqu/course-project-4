using PicsyncClient.Converters.Json;
using PicsyncClient.Models.Albums;
using PicsyncClient.Utils;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public class PictureRemote : PictureBase
{
    // Свойства
    [JsonPropertyName("id"        )] public required ulong  Id    { get; set; }
    [JsonPropertyName("name"      )] public override string? Name { get; set; }
    [JsonPropertyName("hash"      )] public override string? Hash { get; set; }
    [JsonPropertyName("size"      )] public override ulong?  Size { get; set; }
    [JsonPropertyName("width"     )] public override int?   Width { get; set; }
    [JsonPropertyName("height"    )] public override int?  Height { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime?  UploadedAt { get; set; }

    [JsonPropertyName("date")]
    [JsonConverter(typeof(UniversalDateTimeConverter))]
    public override DateTime? Date { get; set; }

    [JsonIgnore]
    public AlbumRemote SpecificAlbum
    {
        get => (AlbumRemote)Album;
        set => Album = value;
    }

    // Геттеры
    public override string OriginalPath  => URLs.PictureOriginal (SpecificAlbum.Id, Id, SpecificAlbum?.Preview?.Signature).ToString();
    public override string ThumbnailPath => URLs.PictureThumbnail(SpecificAlbum.Id, Id, SpecificAlbum?.Preview?.Signature).ToString();
}
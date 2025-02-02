using PicsyncClient.Converters.Json;
using PicsyncClient.Models.Albums;
using PicsyncClient.Utils;
using SQLite;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public class PictureRemote : PictureBase
{
    public PictureRemote() : base() { }

    // ��������
    [JsonPropertyName("id")] [PrimaryKey] public ulong Id { get; set; }
    [JsonPropertyName("name")] public override string Name { get; set; }
    [JsonPropertyName("hash")] public override string Hash { get; set; }
    [JsonPropertyName("size")] public override ulong Size { get; set; }
    [JsonPropertyName("width")] public override int Width { get; set; }
    [JsonPropertyName("height")] public override int Height { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime? UploadedAt { get; set; }

    [JsonPropertyName("date")]
    [JsonConverter(typeof(UniversalDateTimeConverter))]
    public override DateTime Date { get; set; }

    [Ignore]
    [JsonIgnore]
    public new AlbumRemote SpecificAlbum
    {
        get => (AlbumRemote)Album;
        set => Album = value;
    }

    // �������
    public override string OriginalPath  => URLs.PictureOriginal (SpecificAlbum?.Id ?? 0, Id, SpecificAlbum?.Preview?.Signature).ToString();
    public override string ThumbnailPath => URLs.PictureThumbnail(SpecificAlbum?.Id ?? 0, Id, SpecificAlbum?.Preview?.Signature).ToString();
}

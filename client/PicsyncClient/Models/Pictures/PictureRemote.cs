using PicsyncClient.Converters.Json;
using PicsyncClient.Models.Albums;
using PicsyncClient.Utils;
using SQLite;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public class PictureRemote : PictureBase
{
    public PictureRemote() : base() { }

    // Свойства
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
    public AlbumRemote SpecificAlbum
    {
        get => (AlbumRemote)Album;
        set => Album = value;
    }

    // Геттеры
    public override string OriginalPath  => URLs.PictureOriginal (SpecificAlbum?.Id ?? 0, Id, SpecificAlbum?.Preview?.Signature).ToString();
    public override string ThumbnailPath => URLs.PictureThumbnail(SpecificAlbum?.Id ?? 0, Id, SpecificAlbum?.Preview?.Signature).ToString();

    public override bool IsRemote => true;
    public override bool IsRemoteInSyncAlbum    => Album is AlbumSynced;
    public override bool IsRemoteNonOwned       => Album is AlbumRemote album && album.Owner != null;
    public override bool IsRemoteOwned          => Album is AlbumRemote album && album.Owner == null;
    public override bool IsStrictRemote         => true;
    public override bool IsStrictRemoteNonOwned => IsRemoteNonOwned;
    public override bool IsStrictRemoteOwned    => IsRemoteOwned;
}

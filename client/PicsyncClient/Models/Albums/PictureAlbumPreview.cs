using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Albums;

public class PictureAlbumPreview
{
    [JsonPropertyName("sign")] public string?     Signature   { get; set; }
    [JsonPropertyName("ids" )] public List<ulong> PictureIds  { get; set; } = [];
}

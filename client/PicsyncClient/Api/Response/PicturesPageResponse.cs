using PicsyncClient.Models.Pictures;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class PicturesPageResponse
{
    [JsonPropertyName("sign"    )] public string? Signature { get; set; }
    [JsonPropertyName("page"    )] public int?    Page      { get; set; }
    [JsonPropertyName("limit"   )] public int?    Limit     { get; set; }
    [JsonPropertyName("total"   )] public int     Total     { get; set; }
    [JsonPropertyName("pictures")] public List<PictureRemote> Pictures { get; set; } = [];
}
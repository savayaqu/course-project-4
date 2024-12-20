using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class Picture
{
    [JsonPropertyName("id")] public ulong? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("hash")] public string? Hash { get; set; }
    [JsonPropertyName("size")] public ulong? Size { get; set; }
    [JsonPropertyName("date")] public DateTime? Date { get; set; }
    [JsonPropertyName("width")] public int? Width { get; set; }
    [JsonPropertyName("height")] public int? Height { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime? UploadedAt { get; set; }

    public Uri? Thumbnail { get; set; }
}
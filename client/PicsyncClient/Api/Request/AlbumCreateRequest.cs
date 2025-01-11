using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class AlbumCreateRequest(string name, string? path = null)
{
    [JsonPropertyName("name")] public string  Name { get; set; } = name;
    [JsonPropertyName("path")] public string? Path { get; set; } = path;
}

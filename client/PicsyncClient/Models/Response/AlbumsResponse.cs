using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class AlbumsResponse
{
    [JsonPropertyName("own"       )] public List<Album> Own        { get; set; } = [];
    [JsonPropertyName("accessible")] public List<Album> Accessible { get; set; } = [];
}
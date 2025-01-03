using PicsyncClient.Models.Albums;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class AlbumsResponse
{
    [JsonPropertyName("own"       )] public List<AlbumRemote> Own        { get; set; } = [];
    [JsonPropertyName("accessible")] public List<AlbumRemote> Accessible { get; set; } = [];
}
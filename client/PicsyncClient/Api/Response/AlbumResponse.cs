using PicsyncClient.Models.Albums;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class AlbumResponse
{
    [JsonPropertyName("album")] public AlbumRemote Album { get; set; }
}
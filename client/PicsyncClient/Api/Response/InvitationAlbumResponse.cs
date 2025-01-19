using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class InvitationAlbumResponse
{
    [JsonPropertyName("album"   )] public required AlbumRemote         Album    { get; set; }
    [JsonPropertyName("pictures")] public required List<PictureRemote> Pictures { get; set; }
}
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class InvitationAlbumResponse
{
    [JsonPropertyName("album"   )] public AlbumRemote         Album    { get; set; }
    [JsonPropertyName("pictures")] public List<PictureRemote> Pictures { get; set; }
}
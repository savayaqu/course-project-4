using PicsyncClient.Models.Pictures;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class PictureResponse
{
    [JsonPropertyName("picture")] public PictureRemote Picture { get; set; }
}
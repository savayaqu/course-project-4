using PicsyncClient.Models.Pictures;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class PicturesSendResponse
{
    [JsonPropertyName("sign")] public string? Signature { get; set; }
    [JsonPropertyName("successful")] public List<PictureRemote>      Successful { get; set; } = [];
    [JsonPropertyName("errored"   )] public List<ErroredPictureSend> Errored    { get; set; } = [];
}

public class ErroredPictureSend
{
    [JsonPropertyName("name"   )] public string         Name    { get; set; }
    [JsonPropertyName("message")] public string         Message { get; set; }
    [JsonPropertyName("picture")] public PictureRemote? Picture { get; set; }
}
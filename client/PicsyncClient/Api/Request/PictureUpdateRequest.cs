using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class PictureUpdateRequest(string name)
{
    [JsonPropertyName("name")] public string  Name { get; set; } = name;
}

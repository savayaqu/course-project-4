using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class ErrorResponse
{
    [JsonPropertyName("code"   )] public int?    Code    { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("errors" )] public Dictionary<string, List<string>>? Errors { get; set; }
}
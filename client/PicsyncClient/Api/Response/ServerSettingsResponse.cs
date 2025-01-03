using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class ServerSettingsResponse
{
    [JsonPropertyName("settings")] public ServerSettings Settings { get; set; }
}
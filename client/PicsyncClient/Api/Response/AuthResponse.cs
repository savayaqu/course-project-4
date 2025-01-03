using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class AuthResponse
{
    [JsonPropertyName("token")] public string Token { get; set; }
    [JsonPropertyName("user" )] public User   User  { get; set; }
}
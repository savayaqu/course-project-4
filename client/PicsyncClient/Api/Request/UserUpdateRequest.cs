using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class UserUpdateRequest(string? login = null, string? password = null, string? nickname = null)
{
    [JsonPropertyName("login"   )] public string? Login    { get; set; } = login;
    [JsonPropertyName("password")] public string? Password { get; set; } = password;
    [JsonPropertyName("name"    )] public string? Nickname { get; set; } = nickname;
}

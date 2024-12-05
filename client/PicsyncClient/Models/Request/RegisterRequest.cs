using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class RegisterRequest(string login, string password, string nickname)
{
    [JsonPropertyName("login"   )] public string Login    { get; set; } = login;
    [JsonPropertyName("password")] public string Password { get; set; } = password;
    [JsonPropertyName("name"    )] public string Nickname { get; set; } = nickname;
}

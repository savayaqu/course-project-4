using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class CredentialsRequest(string login, string password)
{
    [JsonPropertyName("login"   )] public string Login    { get; set; } = login;
    [JsonPropertyName("password")] public string Password { get; set; } = password;
}

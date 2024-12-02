using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class RegisterRequest
{
    public RegisterRequest(string login, string password, string nickname)
    {
        Login = login;
        Password = password;
        Nickname = nickname;
    }

    [JsonPropertyName("login"   )] public string Login    { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
    [JsonPropertyName("name"    )] public string Nickname { get; set; }
}
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class CredentialsRequest
{
    public CredentialsRequest(string login, string password)
    {
        Login = login;
        Password = password;
    }

    [JsonPropertyName("login"   )] public string Login    { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
}
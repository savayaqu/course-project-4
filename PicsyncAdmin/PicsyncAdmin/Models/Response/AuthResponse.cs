using System.Text.Json.Serialization;

namespace PicsyncAdmin.Models.Response
{
    // Класс описывающий ответ модуля Auth
    public class AuthResponse
    {
        [JsonPropertyName("user")] public User User { get; set; }
        [JsonPropertyName("token")] public string Token { get; set; }
    }
}

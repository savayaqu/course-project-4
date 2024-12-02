using PicsyncClient.Enum;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models
{
    public class User
    {
        [JsonPropertyName("id")]        public ulong Id         { get; set; }
        [JsonPropertyName("login")]     public string? Login    { get; set; }
        [JsonPropertyName("password")]  public string? Password { get; set; }
        [JsonPropertyName("name")]      public string Nickname  { get; set; }
        [JsonPropertyName("role")]      public RoleEnum Role    { get; set; } = RoleEnum.User;
    }
}

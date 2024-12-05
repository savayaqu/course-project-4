using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class User
    {

        // Класс описывающий таблицу users
        [JsonPropertyName("id")]                    public ulong? Id { get; set; }
        [JsonPropertyName("name")]                  public string? Name { get; set; }
        [JsonPropertyName("login")]                 public string? Login {  get; set; }
        [JsonPropertyName("password")]              public string? Password {  get; set; }
        [JsonPropertyName("role")]                  public string? Role { get; set; }
        [JsonPropertyName("is_banned")]             public bool? IsBanned { get; set; }
        [JsonPropertyName("remember_token")]        public string? RememberToken { get; set; }
        [JsonPropertyName("created_at")]            public DateTime? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]            public DateTime? UpdatedAt { get; set; }
    }
}

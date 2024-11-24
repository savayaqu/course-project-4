using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class AuthResponse
    {
        // Класс описывающий ответ модуля Auth
        [JsonPropertyName("user")]              public User User { get; set; }
        [JsonPropertyName("token")]             public string Token { get; set; }
    }
}

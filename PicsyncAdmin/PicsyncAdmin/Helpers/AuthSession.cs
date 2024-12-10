using PicsyncAdmin.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Diagnostics;

namespace PicsyncAdmin.Helpers
{
    public static class AuthSession
    {
        public static User? User { get; set; }
        public static string? Token { get; set; }
        public static string? SelectedUrl { get; set; }

        // Метод для очистки данных при выходе
        public static void ClearSession()
        {
            User = null;
            Token = null;
            SelectedUrl = null;
            Preferences.Remove("User");
            Preferences.Remove("Token");
            Preferences.Remove("SelectedUrl");
        }

        // Метод для восстановления данных из Preferences
        public static void LoadSession()
        {

            // Восстанавливаем токен
            Token = Preferences.Get("Token", null);
            // Восстанавливаем SelectedUrl
            SelectedUrl = Preferences.Get("SelectedUrl", null);
            // Восстанавливаем пользователя только если в Preferences есть данные
            var userJson = Preferences.Get("User", null);
            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    User = JsonSerializer.Deserialize<User>(userJson);
                }
                catch (Exception ex)
                {
                    ClearSession(); // Очистим сессию при ошибке
                }
            }
        }
    }
}

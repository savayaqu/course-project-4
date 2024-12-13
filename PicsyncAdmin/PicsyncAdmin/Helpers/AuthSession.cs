using PicsyncAdmin.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PicsyncAdmin.Helpers
{
    public static class AuthSession
    {
        public static event Action<string?>? OnUrlChanged;
        public static User? User { get; set; }
        public static string? Token { get; set; }
        private static string? _selectedUrl = null;
        public static string? SelectedUrl
        {
            get => _selectedUrl ??= Preferences.Get("SelectedUrl", string.Empty);
            set
            {
                _selectedUrl = value;

                if (value == null)
                    Preferences.Remove("SelectedUrl");
                else
                    Preferences.Set("SelectedUrl", value);
            }
        }

        public static void SaveUrl(string? selectedUrl)
        {
            SelectedUrl = selectedUrl;
            OnUrlChanged?.Invoke(selectedUrl);
        }

        // Метод для очистки данных при выходе
        public static void ClearSession()
        {
            User = null;
            Token = null;
            SelectedUrl = null;
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

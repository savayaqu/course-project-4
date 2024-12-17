using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;
using System.Diagnostics;
using System.Text.Json;


namespace PicsyncAdmin.Helpers
{
    public static class AuthSession
    {
        public static event Action<string?>? OnUrlChanged;
        public static User? User { get; set; }
        private static string? _token = null;
        public static string? Token
        {

            get 
            { 
                var token = _token; 
                if (token == null)
                {
                    token = Preferences.Get("token", string.Empty);
                }
                if (token == string.Empty)
                {
                    return null;
                }
                _token = token;
                return token;
            } 
            set
            {
                _token = value;

                if (value == null)
                    Preferences.Remove("token");
                else
                    Preferences.Set("token", value);
            }
        }
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

        public static void SaveUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL не может быть пустым", nameof(url));

            SelectedUrl = url;
            Preferences.Set("SelectedUrl", url);

            Debug.WriteLine($"URL сохранён в AuthSession: {SelectedUrl}");
        }


        // Метод для очистки данных при выходе
        public static void ClearSession()
        {
            User = null;
            Token = null;

            SelectedUrl = null;
        }

        // Метод для восстановления данных из Preferences
        public static async Task LoadSession()
        {
            // Восстанавливаем SelectedUrl
            SelectedUrl = Preferences.Get("SelectedUrl", string.Empty);
            Debug.WriteLine($"SelectedUrl из Preferences: {SelectedUrl}");

            if (string.IsNullOrWhiteSpace(SelectedUrl))
            {
                await Shell.Current.GoToAsync("///ApiUrlSelectionPage");
                return;
            }

            // Восстанавливаем токен
            Token = Preferences.Get("token", null);

            // Восстанавливаем пользователя только если в Preferences есть данные
            var userJson = Preferences.Get("User", null);
            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    User = JsonSerializer.Deserialize<User>(userJson);
                    await CheckAuthFromServer();
                }
                catch (Exception)
                {
                    ClearSession();
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            }
            else
            {
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }

        //Проверка работоспособности сервера и ещё чето слово умное ну ти пон
        public static async Task CheckAuthFromServer()
        {
            try
            {
                var response = await Fetch.DoAsync(HttpMethod.Get, "/users/me");

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("AUTH: Пользователь авторизован");
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else if ((int)response.StatusCode >= 500)
                {
                    Debug.WriteLine("AUTH: Ошибка сервера");
                    ClearSession();
                    await Shell.Current.GoToAsync("///ApiUrlSelectionPage");
                }
                else
                {
                    Debug.WriteLine("AUTH: Токен недействителен или истёк");
                    ClearSession();
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AUTH: Ошибка проверки авторизации: {ex.Message}");
                ClearSession();
                await Shell.Current.GoToAsync("///ApiUrlSelectionPage");
            }
        }


    }
}

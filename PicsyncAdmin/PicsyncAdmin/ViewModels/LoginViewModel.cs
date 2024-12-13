using System.Diagnostics;
using MvvmHelpers;
using PicsyncAdmin.Models;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models.Response;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace PicsyncAdmin.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? login;
        [ObservableProperty]
        private string? password;
        [ObservableProperty]
        private string? selectedServer = AuthSession.SelectedUrl;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(TryLoginCommand))]
        private bool isFetch = false;

        public LoginViewModel()
        {
            AuthSession.OnUrlChanged += url => SelectedServer = url;
            SelectedServer = AuthSession.SelectedUrl; // Инициализация текущим значением
        }


        [RelayCommand]
        public static async Task SelectServer()
        {
            AuthSession.ClearSession();
            await Shell.Current.GoToAsync("//ApiUrlSelectionPage");
        }
        [RelayCommand]
        private async Task TryLogin()
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                return;
            }
            IsFetch = true;
            var authResponse = await AuthenticateUserAsync(Login, Password);
            if (authResponse != null)
            {
                // После успешной аутентификации сохраняем в Helpers.AuthSession юзера и токен
                AuthSession.User = authResponse.User;
                AuthSession.Token = authResponse.Token;
                Debug.WriteLine(AuthSession.User);
                Debug.WriteLine(AuthSession.Token);
                // Сохраняем в Preferences
                Preferences.Set("User", JsonSerializer.Serialize(authResponse.User));
                Preferences.Set("Token", authResponse.Token);

                await Shell.Current.GoToAsync("//MainPage");
            }
            IsFetch = false;
        }

        private async Task<AuthResponse?> AuthenticateUserAsync(string login, string password)
        {
            var loginData = new { login, password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                var apiUrl = new API_URL("login");
                Debug.WriteLine($"Попытка отправить запрос на: {apiUrl}");

                HttpResponseMessage response = await new HttpClient().PostAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AuthResponse>(content);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Неправильный логин или пароль", "OK");
                }
            }
            catch (InvalidOperationException ex)
            {
                await Shell.Current.DisplayAlert("Ошибка URL", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка сети", ex.Message, "OK");
            }
            return null;
        }

    }
}

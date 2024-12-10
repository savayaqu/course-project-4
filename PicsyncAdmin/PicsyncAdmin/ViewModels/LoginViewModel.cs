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
        private readonly User? _user = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
        [ObservableProperty]
        private string login;
        [ObservableProperty]
        private string password;
        [ObservableProperty]
        private string selectedServer = AuthSession.SelectedUrl;
        [RelayCommand]
        public async Task SelectServer()
        {
            AuthSession.SelectedUrl = null;
            Preferences.Remove("SelectedUrl");
            await Shell.Current.GoToAsync("//ApiUrlSelectionPage");
        }
        //TODO: isfetch для кнопки и блокать и т.д
        [RelayCommand]
        private async Task TryLogin()
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                return;
            }

            var authResponse = await AuthenticateUserAsync(Login, Password);
            if (authResponse != null)
            {
                // После успешной аутентификации сохраняем в Helpers.AuthSession юзера и токен
                AuthSession.User = authResponse.User;
                AuthSession.Token = authResponse.Token;

                // Сохраняем в Preferences
                Preferences.Set("User", JsonSerializer.Serialize(authResponse.User));
                Preferences.Set("Token", authResponse.Token);

                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private async Task<AuthResponse> AuthenticateUserAsync(string login, string password)
        {
            var loginData = new { login, password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await new HttpClient().PostAsync(new API_URL("login"), jsonContent);

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
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка сети", ex.Message, "OK");
            }
            return null;
        }
    }
}

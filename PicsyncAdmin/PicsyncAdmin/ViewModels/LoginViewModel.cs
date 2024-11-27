using MvvmHelpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace PicsyncAdmin.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _login;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public ICommand LoginCommand { get; }
        public ICommand CheckTokenCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            CheckTokenCommand = new Command(OnCheckToken);
        }
       
        // Логика для проверки токена и наличия пользователя
        private async void OnCheckToken()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            var userJson = await SecureStorage.GetAsync("auth_user");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userJson))
            {
                // Десериализация пользователя
                var user = JsonSerializer.Deserialize<User>(userJson);

                // Переход на главную страницу
                await App.Current.MainPage.Navigation.PushAsync(new Home(user, token));
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Ошибка", "Срок действия токена истёк", "OK");
            }
        }


        private async void OnLoginClicked()
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                await App.Current.MainPage.DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                return;
            }

            var authResponse = await AuthenticateUserAsync(Login, Password);
            if (authResponse != null)
            {
                // Сохраняем токен в защищённое хранилище
                await SecureStorage.SetAsync("auth_token", authResponse.Token);

                // Сохраняем пользователя в защищённое хранилище
                var userJson = JsonSerializer.Serialize(authResponse.User);
                await SecureStorage.SetAsync("auth_user", userJson);

                // Переход на главную страницу
                await App.Current.MainPage.Navigation.PushAsync(new Home(authResponse.User, authResponse.Token));
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
                    await App.Current.MainPage.DisplayAlert("Ошибка", "Неправильный логин или пароль", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Ошибка сети", ex.Message, "OK");
            }
            return null;
        }
    }
}

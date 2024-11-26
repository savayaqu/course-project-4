using MvvmHelpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
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

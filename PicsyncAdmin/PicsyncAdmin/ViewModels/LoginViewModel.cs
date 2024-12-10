using MvvmHelpers;
using PicsyncAdmin.Models;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models.Response;

namespace PicsyncAdmin.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly User? _user = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
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

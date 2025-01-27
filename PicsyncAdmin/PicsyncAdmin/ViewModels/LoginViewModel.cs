using System.Diagnostics;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models.Response;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;
using System.Text.Json;

namespace PicsyncAdmin.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? login;

        [ObservableProperty]
        private string? password;
        [ObservableProperty]
        private string? error;
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
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                return;
            }

            IsFetch = true;

            try
            {
                var loginData = new { login = Login, password = Password };
                var (response, authResponse) = await Fetch.DoAsync<AuthResponse>(
                    HttpMethod.Post,
                    "login",
                    setIsFetch: isFetching => IsFetch = isFetching,
                    setError: message => Error = message,
                    body: loginData,
                    serialize: true
                );
                if (Error != null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", Error, "OK");
                    return;
                }
                if (authResponse == null || !response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Неправильный логин или пароль", "OK");
                    return;
                }
                if(authResponse.User.Role == "admin")
                {
                    // Сохраняем пользователя и токен в AuthSession
                    AuthSession.User = authResponse.User;
                    AuthSession.Token = authResponse.Token;

                    Debug.WriteLine($"Пользователь: {AuthSession.User}");
                    Debug.WriteLine($"Токен: {AuthSession.Token}");

                    // Сохраняем в Preferences
                    Preferences.Set("User", JsonSerializer.Serialize(authResponse.User));
                    Preferences.Set("Token", authResponse.Token);

                    Debug.WriteLine("Навигация на MainPage...");
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Доступ запрещен", "OK");
                }
                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Что-то пошло не так: {ex.Message}", "OK");
                Debug.WriteLine($"Неизвестная ошибка: {ex}");
            }
        }
    }
}

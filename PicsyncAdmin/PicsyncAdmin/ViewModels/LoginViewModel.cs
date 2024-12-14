using System.Diagnostics;
using MvvmHelpers;
using PicsyncAdmin.Models;
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
                await ShowAlert("Ошибка", "Введите логин и пароль");
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
                    await ShowAlert("Ошибка", Error);
                    return;
                }
                if (authResponse == null || !response.IsSuccessStatusCode)
                {
                    await ShowAlert("Ошибка", "Неправильный логин или пароль");
                    return;
                }
                
                // Сохраняем пользователя и токен в AuthSession
                AuthSession.User = authResponse.User;
                AuthSession.Token = authResponse.Token;

                Debug.WriteLine($"Пользователь: {AuthSession.User}");
                Debug.WriteLine($"Токен: {AuthSession.Token}");

                // Сохраняем в Preferences
                Preferences.Set("User", JsonSerializer.Serialize(authResponse.User));
                Preferences.Set("Token", authResponse.Token);

                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                await ShowAlert("Ошибка", $"Что-то пошло не так: {ex.Message}");
                Debug.WriteLine($"Неизвестная ошибка: {ex}");
            }
        }

        // Универсальный метод для отображения ошибок
        private static Task ShowAlert(string title, string message)
        {
            return Shell.Current.DisplayAlert(title, message, "OK");
        }
    }
}

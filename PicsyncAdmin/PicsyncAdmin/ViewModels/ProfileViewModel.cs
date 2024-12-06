using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using PicsyncAdmin.Helpers;
using Microsoft.Maui.Graphics;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Models.Response;

namespace PicsyncAdmin.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly string? _token = AuthSession.Token;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? login;

        [ObservableProperty]
        private string? password;

        [ObservableProperty]
        private string? confirmPassword;

        [ObservableProperty]
        private string? validationMessage;
        [ObservableProperty]
        private  Color? validationMessageColor;

        public ProfileViewModel()
        {
            _httpClient = new HttpClient();
            // Инициализация свойств данными из текущей сессии
            Name = AuthSession.User?.Name;
            Login = AuthSession.User?.Login;
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            ValidationMessage = null;
            ValidationMessageColor = Colors.Red; // По умолчанию ошибки будут красными
            try
            {
                if (Password != ConfirmPassword)
                { 
                     ValidationMessage = "Пароли не совпадают"; 
                    return;
                }
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                var payload = new { Name, Login, Password };
                var response = await _httpClient.PostAsJsonAsync(new API_URL("/users/me"), payload);

                if (response.IsSuccessStatusCode)
                {
                    // Обновляем данные пользователя в сессии
                    AuthSession.User!.Name = Name;
                    AuthSession.User!.Login = Login;

                    ValidationMessage = "Изменения сохранены успешно.";
                    ValidationMessageColor = Colors.Green; // Успешное сообщение зелёным

                    // Очищаем сообщение через 3 секунды
                    await Task.Delay(3000);
                    ValidationMessage = null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
                    if (errorResponse?.Errors != null)
                    {
                        var errorMessages = errorResponse.Errors
                            .SelectMany(err => err.Value)
                            .Aggregate((current, next) => $"{current}\n{next}");

                        ValidationMessage = errorMessages;
                    }
                    else
                    {
                        ValidationMessage = "Ошибка валидации.";
                    }
                }
                else
                {
                    ValidationMessage = "Не удалось сохранить изменения.";
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка: {ex.Message}";
            }
        }



        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                await _httpClient.PostAsync(new API_URL("/logout"), null);

                AuthSession.ClearSession();
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при выходе: {ex.Message}");
            }
        }
    }
}

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Helpers;

namespace PicsyncAdmin.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty] private string? name;

        [ObservableProperty] private string? login;

        [ObservableProperty] private string? password;

        [ObservableProperty] private string? confirmPassword;

        [ObservableProperty] private string? validationMessage;

        [ObservableProperty] private Color? validationMessageColor;

        public ProfileViewModel()
        {
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
                var payload = new { Name, Login, Password };
                var response = await Fetch.DoAsync(HttpMethod.Post, "/users/me", setError: msg => ValidationMessage = msg, body: payload, serialize: true);
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
                else
                {
                    ValidationMessage ??= "Не удалось сохранить изменения.";
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
            bool isExit = false;
            try
            {
                var response = await Fetch.DoAsync(HttpMethod.Post, "/logout", setError: msg => Debug.WriteLine(msg));
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ошибка {response.StatusCode}");
                }

                isExit = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при выходе: {ex.Message}");

                isExit = await Shell.Current.DisplayAlert(
                    "Не получилось выйти",
                    $"Вы желаете насильно выйти? \n{ex.Message}",
                    "Да",
                    "Нет"
                );
            }
            if (!isExit) return;
            AuthSession.ClearSession();
            await Shell.Current.GoToAsync("//ApiUrlSelectionPage");
        }
    }
}

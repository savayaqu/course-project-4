using PicsyncAdmin.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using PicsyncAdmin.Resources;
using Microsoft.Win32;
using System.Net.Http.Json;

namespace PicsyncAdmin.Views.Auth;

public partial class Login : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();

    public Login()
    {
        InitializeComponent();
    }

    // Переход на страницу регистрации

    //private async void OnRegisterTapped(object sender, EventArgs e)
    //{
    //   await Navigation.PushAsync(new Register());
    //}

    // Аутентификация
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Получение логина и пароля из формы
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
            return;
        }

        var loginResponse = await AuthenticateUserAsync(login, password);
        if (loginResponse != null)
        {
            // Переход на страницу Home с данными пользователя
            await Navigation.PushAsync(new Home(loginResponse.User, loginResponse.Token));
        }
    }

    // Аутентификация пользователя
    private async Task<AuthResponse> AuthenticateUserAsync(string login, string password)
    {
        // Формирование тела для отправки
        var loginData = new { login, password };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        try
        {
            // Отправка POST-запроса на сервер
            HttpResponseMessage response = await _httpClient.PostAsync(new API_URL("login"), jsonContent);

            // Проверка кода ответа
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
                    return result; // Возвращаем данные пользователя и токен
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Ошибка входа", "Неправильный логин или пароль", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Произошла ошибка на сервере", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", ex.Message, "OK");
        }
        return null;
    }
}
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views.Auth;
using System.Net;
using System.Net.Http;

namespace PicsyncAdmin.Views;

public partial class Home : ContentPage
{
	private User _user;
	private string _token;
    private readonly HttpClient _httpClient = new HttpClient();

    public Home(User user, string token)
	{
		InitializeComponent();
		_user = user;
		_token = token;

		//Устанавливаем аватар и никнейм
		LoginHome.Text = _user.Login;

    }

    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        await Logout();

    }
    private async Task Logout()
    {

        // Проверка токена перед отправкой запроса
        string token = _token;
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Ошибка", "Токен не найден. Возможно, вы уже вышли из системы.", "OK");
            return;
        }
        // Добавляем токен в заголовок Authorization
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        await DisplayAlert("dff", "sd", "dfg");

        // Отправка POST-запроса на сервер
        HttpResponseMessage response = await _httpClient.PostAsync(new API_URL("logout"), null);

        // Проверка кода ответа
        if (response.IsSuccessStatusCode)
        {

            await DisplayAlert("Выход", "Вы успешно вышли из системы", "OK");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            // Возврат на корневую страницу
            await Navigation.PushAsync(new Login());
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await DisplayAlert("Ошибка", "Сессия недействительна. Пожалуйста, войдите снова.", "OK");
        }
        else
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка на сервере: {response.StatusCode}", "OK");
        }
    }           
}

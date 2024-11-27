using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views;
using PicsyncAdmin.Views.Auth;
using System.Diagnostics;
using System.Text.Json;

namespace PicsyncAdmin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Назнаение AppShell в качестве главной страницы
            MainPage = new AppShell();
        }
        protected override async void OnStart()
        {
            base.OnStart();

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    // Если токен отсутствует, переходим на страницу логина
                    MainPage = new NavigationPage(new Login());
                }
                else
                {
                    // (Опционально) Можно добавить проверку действительности токена, запросив данные с сервера
                    var user = await FetchUserByToken(token);

                    if (user == null)
                    {
                        // Если пользователь не найден, переходим на страницу логина
                        MainPage = new NavigationPage(new Login());
                    }
                    else
                    {
                        // Если токен и пользователь действительны, переходим на главную страницу
                        MainPage = new NavigationPage(new Home(user, token));
                    }
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки переходим на страницу логина
                await App.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось выполнить проверку токена: {ex.Message}", "OK");
                MainPage = new NavigationPage(new Login());
            }
        }
        private async Task<User> FetchUserByToken(string token)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(new API_URL("users/me"));
                if (response.IsSuccessStatusCode)
                {
                    var userJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<User>(userJson);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке токена: {ex.Message}");
            }
            return null; // Возвращаем null, если запрос не удался
        }
        


    }
}

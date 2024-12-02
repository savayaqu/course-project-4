using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PicsyncAdmin.Models;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Resources;
using System.Diagnostics;

namespace PicsyncAdmin.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public User User { get; set; } = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
        private readonly HttpClient _httpClient;
        public ICommand LogoutCommand { get; }

        public ProfileViewModel()
        {
            Debug.WriteLine(User.Name);
            _httpClient = new HttpClient();
            LogoutCommand = new Command(OnLogoutClicked);

        }
        // Обработчик кнопки выхода
        private async void OnLogoutClicked()
        {
            // Добавляем токен в заголовок Authorization
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            // Отправка POST-запроса на сервер
            HttpResponseMessage response = await _httpClient.PostAsync(new API_URL("logout"), null);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            // Очистка сессии (юзер и токен)
            AuthSession.ClearSession();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}

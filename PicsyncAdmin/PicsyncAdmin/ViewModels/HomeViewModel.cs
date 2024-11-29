using Microsoft.Maui.Controls.PlatformConfiguration;
using MvvmHelpers;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views.Auth;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows.Input;

namespace PicsyncAdmin.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly User? _user = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
        private readonly HttpClient _httpClient;

        public ObservableCollection<Complaint> Complaints { get; } = new();

        public ICommand LogoutCommand { get; }
        public ICommand LoadComplaintsCommand { get; }


        public HomeViewModel()
        {
            _httpClient = new HttpClient();
            LoginViewModel.OnCheckToken();
            // Инициализация команд
            LogoutCommand = new Command(OnLogoutClicked);
            LoadComplaintsCommand = new Command(async () => await LoadComplaintsAsync());
            // Переносим вызов LoadComplaintsAsync() в асинхронный метод после инициализации
            _ = LoadComplaintsAsync();
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

        private async Task LoadComplaintsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Установка заголовка авторизации
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // Получение жалоб
                var response = await _httpClient.GetFromJsonAsync<ComplaintResponse>(new API_URL("complaints"));

                if (response?.Complaints != null)
                {
                    Shell.Current.Dispatcher.Dispatch(() =>
                    {
                        Complaints.Clear();
                        foreach (var complaint in response.Complaints)
                        {
                            if (complaint.Picture != null)
                            {
                                // Генерация пути для картинки
                                complaint.Picture.Path ??=
                                    new API_URL($"/albums/{complaint.Album?.Id}/pictures/{complaint.Picture.Id}/original?sign={complaint.Sign}");
                            }
                            Complaints.Add(complaint);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке жалоб: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить жалобы: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

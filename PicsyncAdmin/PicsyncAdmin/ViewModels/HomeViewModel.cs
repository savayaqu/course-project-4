using MvvmHelpers;
using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views.Auth;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Windows.Input;

namespace PicsyncAdmin.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly User _user;
        private readonly string _token;
        private readonly HttpClient _httpClient;

        public ObservableCollection<Complaint> Complaints { get; } = new();

        public ICommand LogoutCommand { get; }
        public ICommand LoadComplaintsCommand { get; }


        public HomeViewModel(User user, string token)
        {
            _user = user;
            _token = token;
            _httpClient = new HttpClient();


            // Инициализация команд
            LogoutCommand = new Command(OnLogoutClicked);
            LoadComplaintsCommand = new Command(async () => await LoadComplaintsAsync());

            // Переносим вызов LoadComplaintsAsync() в асинхронный метод после инициализации
            _ = LoadComplaintsAsync();
        }

        private async void OnLogoutClicked()
        {
            // Выход из аккаунта
            bool response = await MethodLogout.Logout(_user, _token);
            if (response)
            {
                // Возвращаемся на экран логина
                await App.Current.MainPage.Navigation.PushAsync(new Login());
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Ошибка выхода", "Не удалось выйти из аккаунта", "ОК");
            }
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
                    Application.Current.Dispatcher.Dispatch(() =>
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
                await App.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось загрузить жалобы: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

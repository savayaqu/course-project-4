
using MvvmHelpers;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Input;

namespace PicsyncAdmin.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly User? _user = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
        private readonly HttpClient _httpClient;
        // Привязка данных из AppSettings для UI
        public int UsedPercent => AppSettings.UsedPercent;
        public int UploadDisablePercentage => AppSettings.UploadDisablePercentage;
        public long TotalSpace => AppSettings.TotalSpace;
        public long FreeSpace => AppSettings.FreeSpace;
        public long UsedSpace => AppSettings.UsedSpace;
        public ObservableCollection<Complaint> Complaints { get; } = new();

        public ICommand LoadComplaintsCommand { get; }


        public HomeViewModel()
        {
            _httpClient = new HttpClient();
            LoginViewModel.OnCheckToken();
            // Инициализация команд
            LoadComplaintsCommand = new Command(async () => await LoadComplaintsAsync());
            // Переносим вызов LoadComplaintsAsync() в асинхронный метод после инициализации
            _ = LoadComplaintsAsync();
            _ = LoadSettings();
        }
        public async Task LoadSettings()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(new API_URL("/settings"));
                var settingsResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                if (settingsResponse != null && settingsResponse.Settings != null && settingsResponse.Space != null)
                {
                    // Сохраняем данные в AppSettings
                    AppSettings.UploadDisablePercentage = settingsResponse.Settings.UploadDisablePercentage;
                    AppSettings.TotalSpace = settingsResponse.Space.Total;
                    AppSettings.FreeSpace = settingsResponse.Space.Free;
                    AppSettings.UsedSpace = settingsResponse.Space.Used;
                    AppSettings.UsedPercent = settingsResponse.Space.UsedPercent;
                }
                else
                {
                    Debug.WriteLine("Ошибка при десериализации ответа или отсутствуют нужные данные.");
                }

                // Для отладки выводим полученные данные
                Debug.WriteLine($"Settings: {settingsResponse?.Settings?.UploadDisablePercentage}");
                Debug.WriteLine($"Space Total: {settingsResponse?.Space?.Total}");
                Debug.WriteLine($"Space Free: {settingsResponse?.Space?.Free}");
                Debug.WriteLine($"Space Used: {settingsResponse?.Space?.Used}");
                Debug.WriteLine($"Space UsedPercent: {settingsResponse?.Space?.UsedPercent}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке данных настроек: {ex.Message}");
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
                var response = await _httpClient.GetFromJsonAsync<ComplaintResponse>(new API_URL("complaints?status=null"));

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
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить жалобы: {ex.Message}", "ОК");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

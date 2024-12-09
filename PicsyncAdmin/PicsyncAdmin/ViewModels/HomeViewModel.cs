
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using MvvmHelpers;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Input;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace PicsyncAdmin.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        public static HomeViewModel Instance { get; private set; }

        private readonly User? _user = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
        private readonly HttpClient _httpClient;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadComplaintsCommand))]
        private bool isFetch = false;
        [ObservableProperty]
        private double usedPercent;
        [ObservableProperty]
        public bool canLoadMore = false;
        [ObservableProperty]
        private int currentPage = 1;
        [ObservableProperty]
        private string usedSpaceHumanReadable;
        [ObservableProperty]
        private string totalSpaceHumanReadable;
        [ObservableProperty]
        private string freeSpaceHumanReadable;
        [ObservableProperty]
        private double usedPercentDisplay;

        // Конструктор
        public HomeViewModel()
        {
            Instance = this;
            _httpClient = new HttpClient();
            // Подписка на событие обновления настроек
            AppSettings.SettingsUpdated += OnSettingsUpdated;
            _ = LoadComplaints();
            _ = LoadSettings();
        }
        [RelayCommand]
        private async Task NavigateToUserContentPage(Complaint complaint)
        {
            if (complaint == null || complaint.Album == null)
                return;

            var albumId = complaint.Album.Id;
            var picturePath = complaint.Picture?.Path;

            await Shell.Current.Navigation.PushAsync(new UserContentPage(complaint));
        }
        [RelayCommand]
        public async Task ResetComplaints()
        {
            Shell.Current.Dispatcher.Dispatch(() =>
            {
                Complaints.Clear(); // Очистка списка жалоб
                CurrentPage = 1;    // Сброс текущей страницы
                CanLoadMore = false; // Сброс состояния загрузки дополнительных данных
            });
            await LoadComplaints();
        }


        // Обработчик события обновления
        private void OnSettingsUpdated()
        {
            UsedPercentDisplay = AppSettings.UsedPercent;
            UsedPercent = (double)AppSettings.UsedPercent / 100;
            UsedSpaceHumanReadable = AppSettings.BytesToHuman(AppSettings.UsedSpace);
            TotalSpaceHumanReadable = AppSettings.BytesToHuman(AppSettings.TotalSpace);
            FreeSpaceHumanReadable = AppSettings.BytesToHuman(AppSettings.FreeSpace);
        }
        public ObservableCollection<Complaint> Complaints { get; } = new();


        [RelayCommand]
        public async Task LoadSettings()
        {
                var response = await _httpClient.GetStringAsync(new API_URL("/settings"));
                var settingsResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                AppSettings.UploadDisablePercentage = settingsResponse.Settings.UploadDisablePercentage;
                AppSettings.TotalSpace = settingsResponse.Space.Total;
                AppSettings.FreeSpace = settingsResponse.Space.Free;
                AppSettings.UsedSpace = settingsResponse.Space.Used;
                AppSettings.UsedPercent = settingsResponse.Space.UsedPercent;
        }
        [RelayCommand(CanExecute = nameof(CanLoadComplaints))]
        public async Task LoadComplaints()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                IsFetch = true;
                var response = await _httpClient.GetFromJsonAsync<ComplaintResponse>(new API_URL($"complaints?status=null&page={CurrentPage}&limit=5"));
                IsFetch = false;

                if (response?.Complaints != null)
                {
                    Shell.Current.Dispatcher.Dispatch(() =>
                    {
                        CanLoadMore = response.Total > response.Page * response.Limit;
                        CurrentPage++;

                        foreach (var albumData in response.Complaints)
                        {
                            foreach (var complaint in albumData.Complaints)
                            {
                                if (complaint.Picture != null)
                                {
                                    complaint.Picture.Path ??= new API_URL($"/albums/{complaint.Album?.Id}/pictures/{complaint.Picture.Id}/original?sign={complaint.Sign}");
                                }
                                Complaints.Add(complaint);
                            }
                        }
                    });
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Deserialization Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error: {ex.Message}");
            }
        }




        private bool CanLoadComplaints() =>
            !IsFetch;


    }
}

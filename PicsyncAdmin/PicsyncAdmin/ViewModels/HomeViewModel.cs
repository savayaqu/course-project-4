
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
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
        [ObservableProperty]
        private ObservableCollection<AlbumComplaintData> albumComplaints = new();
        [ObservableProperty]
        public ObservableCollection<AlbumViewModel> albums = new();

        // TODO: выводить статус загрузки во время жалоб и если жалоб нет то писать что жалоб нет
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
        private async Task NavigateToUserContentPage(AlbumViewModel album)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new UserContentPage(album));
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Error", e.Message, "OK");
                throw;
            }
        }
        [RelayCommand]
        public async Task ResetComplaints()
        {
            Albums.Clear(); // Очистка списка жалоб
            CurrentPage = 1;    // Сброс текущей страницы
            CanLoadMore = false; // Сброс состояния загрузки дополнительных данных
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
                            var albumViewModel = new AlbumViewModel
                            {
                                AlbumName = albumData.Album.Name,
                                Id = albumData.Album.Id,
                                ComplaintsCount = albumData.ComplaintsCount,
                                Pictures = new ObservableCollection<Picture>(
                                    albumData.Complaints
                                        .Where(c => c.Picture != null)
                                        .Select(c => new Picture
                                        {
                                            Id = c.Picture.Id,
                                            Path = new API_URL($"/albums/{albumData.Album?.Id}/pictures/{c.Picture.Id}/thumb/q480?sign={c.Sign}")
                                        })
                                ),
                                RepresentativeComplaint = albumData.Complaints.FirstOrDefault(),
                                AllComplaints = albumData.Complaints,
                                
                            };

                            Albums.Add(albumViewModel);
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

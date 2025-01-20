
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;
using System.Linq;
using PicsyncAdmin.Models.Response.PicsyncAdmin.Models.Response;
using ApiResponse = PicsyncAdmin.Models.ApiResponse;

namespace PicsyncAdmin.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        public static HomeViewModel? Instance { get; private set; }

        private readonly User? _user = AuthSession.User;
        private readonly string? _token = AuthSession.Token;
        private readonly HttpClient _httpClient;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadComplaintsCommand))]
        private bool isFetch = false;
        [ObservableProperty]
        private string statusMessage;
        [ObservableProperty]
        private double usedPercent;
        [ObservableProperty]
        private string usedSpaceHumanReadable;
        [ObservableProperty]
        private string totalSpaceHumanReadable;
        [ObservableProperty]
        private string freeSpaceHumanReadable;
        [ObservableProperty]
        private double usedPercentDisplay;
        [ObservableProperty]
        public bool canLoadMore = false;
        [ObservableProperty]
        private int currentPage = 1;
       
        public ObservableCollection<AlbumComplaintData> Albums { get; set; } = new ObservableCollection<AlbumComplaintData>();
        public HomeViewModel()
        {
            Instance = this;
            _httpClient = new HttpClient();
            // Подписка на событие обновления настроек
            AppSettings.SettingsUpdated += OnSettingsUpdated;
            // Загрузка настроек
            Task.Run(() => LoadSettings());
            Task.Run(() => LoadComplaints());

        }
        [RelayCommand(CanExecute = nameof(CanLoadComplaints))]
        public async Task LoadComplaints()
        {
            try
            {
                StatusMessage = "Загрузка жалоб...";
                IsFetch = true;

                // Отправляем запрос на сервер
                var response = await Fetch.DoAsync(
                    HttpMethod.Get,
                    $"/complaints?status=null&limit_per_album=3&page={CurrentPage}&limit=3",
                    setError: msg => Debug.WriteLine($"Error: {msg}")
                );

                IsFetch = false;

                if (!response.IsSuccessStatusCode)
                {
                    StatusMessage = "Ошибка загрузки жалоб. Попробуйте снова.";
                    return;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<ComplaintResponse>(responseString);

                if (responseObject == null || responseObject.Albums == null)
                {
                    StatusMessage = "Жалоб нет.";
                    return;
                }

                StatusMessage = ""; // Скрываем статус, если жалобы есть

                // Устанавливаем значение CanLoadMore для определения возможности подгрузки данных
                CanLoadMore = responseObject.Total > responseObject.Page * responseObject.Limit;
                CurrentPage++;

                // Обрабатываем данные из API
                foreach (var albumData in responseObject.Albums)
                {
                    if (albumData == null || albumData.Complaints == null)
                    {
                        continue; // Пропускаем null значения
                    }

                    // Создаем объект Album, если он отсутствует
                    albumData.Album ??= new Album
                    {
                        Id = albumData.Id,
                        Name = albumData.Name,
                        User = albumData.Complaints?.FirstOrDefault()?.FromUser ?? new User
                        {
                            Id = 0,
                            Name = "Неизвестный пользователь"
                        }
                    };

                    // Обрабатываем пути для картинок в жалобах, если они существуют
                    foreach (var complaint in albumData.Complaints?.Where(c => c.Picture != null) ?? Enumerable.Empty<Complaint>())
                    {
                        // Убедитесь, что albumData.Album не равен null
                        if (albumData.Album != null && complaint.Picture != null)
                        {
                            complaint.Picture.Path = new API_URL($"/albums/{albumData.Album.Id}/pictures/{complaint.Picture.Id}/thumb/q480?sign={complaint.Sign}");
                        }
                    }

                    // Добавляем объект albumData в коллекцию Albums
                    Albums.Add(albumData);
                }


                Debug.WriteLine($"Successfully loaded page {responseObject.Page} with {responseObject.Albums.Count} albums.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                StatusMessage = "Ошибка обработки данных.";
            }
            catch (Exception)
            {
                StatusMessage = "Произошла ошибка. Попробуйте снова.";
            }
        }
        private bool CanLoadComplaints() =>
            !IsFetch;

        [RelayCommand]
        private async Task NavigateToUserContentPage(Album album)
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
            try
            {
                // Отправляем запрос через Fetch.DoAsync
                var response = await Fetch.DoAsync(
                    HttpMethod.Get,
                    "/settings", // Путь запроса
                    setError: msg => Debug.WriteLine($"Error: {msg}") // Обработчик ошибок
                );
                var responseString = await response.Content.ReadAsStringAsync();
                var settingsResponse = System.Text.Json.JsonSerializer.Deserialize<SettingsResponse>(responseString);

                if (settingsResponse == null)
                {
                    bool accept = await Shell.Current.DisplayAlert("Ошибка загрузки настроек", "Желаете попробовать снова?", "Да", "Нет");
                    if (accept)
                    {
                        await LoadSettings();
                    }
                    return;
                }

                // Обновление значений настроек
                AppSettings.UploadDisablePercentage = settingsResponse.Settings.UploadDisablePercentage;
                AppSettings.TotalSpace = settingsResponse.Space.Total;
                AppSettings.FreeSpace = settingsResponse.Space.Free;
                AppSettings.UsedSpace = settingsResponse.Space.Used;
                AppSettings.UsedPercent = settingsResponse.Space.UsedPercent;

                // Вызываем событие обновления настроек
                AppSettings.SettingsUpdated += OnSettingsUpdated;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert($"Error loading settings", ex.Message, "OK");
            }
        }



    }
}
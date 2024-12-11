using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using Newtonsoft.Json;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace PicsyncAdmin.ViewModels
{
    public partial class UserContentViewModel : ObservableObject
    {
        public static UserContentViewModel Instance { get; private set; }

        private readonly HttpClient _httpClient;
        private readonly string? _token = AuthSession.Token;
        public AlbumViewModel Album { get; set; }
        public UserContentViewModel(AlbumViewModel album)
        {
            Instance = this;
            _httpClient = new ();
            Album = album;
        }
        [ObservableProperty]
        private ObservableCollection<Picture> albumPictures = new();
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
        private bool isFetch = false;
        [ObservableProperty]
        public bool canLoadMore = false;
        [ObservableProperty]
        public int currentPage = 1;
        [ObservableProperty]
        private string? mainImagePath;
        [ObservableProperty]
        private bool isMainImageVisible;
        [RelayCommand]
        public async Task IssueWarning()
        {
            string comment = await Shell.Current.DisplayPromptAsync("Создание предупреждения", "Комментарий");
            //создание предупреждения
            var response = await _httpClient.PostAsJsonAsync(new API_URL($"/users/{Album.RepresentativeComplaint.AboutUser.Id}/warnings"), new { Comment = comment });
            //перевод жалобы в статус просмотрено
            var complaintResponse = await _httpClient.PostAsJsonAsync(new API_URL($"/complaints/{Album.RepresentativeComplaint.Id}"), new { Status = 1 });

            if (response.IsSuccessStatusCode & complaintResponse.IsSuccessStatusCode)
            {

                await Shell.Current.DisplayAlert("Успех", "Предупреждение выдано", "OK");
                await HomeViewModel.Instance.ResetComplaints();
                await Shell.Current.Navigation.PopModalAsync(); // Закрываем модальное окно
            }
            // TODO: для бана и предупреждения переводить все жалобы в статус выполнен и возвращаться на страницу главную
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось выдать предупреждение", "OK");
            }
        }
        [RelayCommand]
        public async Task DeleteAlbum()
        {
            var result = await Shell.Current.DisplayAlert(
                "Подтверждение удаления",
                "Вы уверены, что хотите удалить этот альбом?",
                "Да", "Нет");
            if (result)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                var response = await _httpClient.DeleteAsync((new API_URL($"/albums/{Album.Id}")));
                if (response.IsSuccessStatusCode)
                {
                    await LoadData(); //Загружаем жалобы
                    await Shell.Current.GoToAsync("//HomePage"); // Возвращаем на HomePage
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить альбом", "OK");
                }
            }
        }
        [RelayCommand]
        public async Task BlockUser()
        {
            var result = await Shell.Current.DisplayAlert(
                "Подтверждение блокировки",
                "Вы уверены, что хотите заблокировать этого пользователя?",
                "Да", "Нет");

            if (result)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                var response = await _httpClient.PostAsJsonAsync(new API_URL($"/users/{Album.RepresentativeComplaint.AboutUser.Id}"), new { is_banned = true });
                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Успех", "Пользователь заблокирован", "OK");
                    await HomeViewModel.Instance.ResetComplaints();
                    await Shell.Current.Navigation.PopModalAsync(); // Закрываем модальное окно
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось заблокировать пользователя", "OK");
                }
            }
        }
        
        [RelayCommand]
        public async Task RejectComplaint()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.DeleteAsync(new API_URL($"/complaints/{Album.RepresentativeComplaint.Id}"));
            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.GoToAsync("//HomePage"); // Возвращаем на HomePage
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось отклонить жалобу", "OK");
            }
        }
        [RelayCommand]
        public async Task ViewImage(Picture picture)
        {
            await Shell.Current.Navigation.PushModalAsync(new FullScreenImagePage(picture, Album.Id));
        }
        [RelayCommand(CanExecute = nameof(CanLoadData))]

        public async Task LoadData()
        {
            if (Album.Id == 0)
                return;

            try
            {
                // Запрос к API для получения списка картинок
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                IsFetch = true;
                var response = await _httpClient.GetStringAsync(new API_URL($"/albums/{Album.Id}/pictures?page={CurrentPage}"));
                IsFetch = false;
                // Десериализация ответа в объект PicturesResponse
                var responseObject = JsonConvert.DeserializeObject<PicturesResponse>(response);
                if (responseObject?.Pictures == null)
                    return;

                CanLoadMore = responseObject.Total > responseObject.Page * responseObject.Limit;
                CurrentPage++;
                // Генерация пути для картинок
                foreach (var picture in responseObject.Pictures)
                {
                    if(picture.Height/picture.Width >=2.5)
                    {
                        picture.OriginalPath ??= new API_URL($"/albums/{Album.Id}/pictures/{picture.Id}/thumb/h1080?sign={responseObject.Sign}");
                    }
                    else if (picture.Width / picture.Height >= 2.5)
                    {
                        picture.OriginalPath ??= new API_URL($"/albums/{Album.Id}/pictures/{picture.Id}/thumb/w1080?sign={responseObject.Sign}");
                    }
                    else
                    {
                        picture.OriginalPath ??= new API_URL($"/albums/{Album.Id}/pictures/{picture.Id}/original?sign={responseObject.Sign}");
                    }
                    picture.Path ??= new API_URL($"/albums/{Album.Id}/pictures/{picture.Id}/thumb/q480?sign={responseObject.Sign}");
                }
                // Добавляем оставшиеся картинки
                foreach (var picture in responseObject.Pictures)
                {
                    AlbumPictures.Add(picture);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибок
                Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private bool CanLoadData() =>
            !IsFetch;
    }
}

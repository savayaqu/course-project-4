using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace PicsyncAdmin.ViewModels
{
    public partial class UserContentViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly string? _token = AuthSession.Token;
        public Complaint Complaint { get; set; }
        public UserContentViewModel(Complaint complaint)
        {
            _httpClient = new ();
            Complaint = complaint;
        }
        [ObservableProperty]
        private ObservableCollection<Picture> albumPictures = new();

        [ObservableProperty]
        private string? mainImagePath;
        [ObservableProperty]
        private bool isMainImageVisible;
        [RelayCommand]
        public async Task IssueWarning()
        {
            string comment = await Shell.Current.DisplayPromptAsync("Создание предупреждения", "Комментарий");
            var response = await _httpClient.PostAsJsonAsync(new API_URL($"/users/{Complaint.AboutUser.Id}/warnings"), new { Comment = comment });
            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Успех", "Предупреждение выдано", "OK");
            }
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
                var response = await _httpClient.DeleteAsync(new API_URL($"/albums/{Complaint.Album.Id}"));
                if (response.IsSuccessStatusCode)
                {
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
                var response = await _httpClient.PostAsJsonAsync(new API_URL($"/users/{Complaint.AboutUser.Id}"), new { is_banned = true });
                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Успех", "Пользователь заблокирован", "OK");
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
            var response = await _httpClient.DeleteAsync(new API_URL($"/complaints/{Complaint.Id}"));
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
            await Shell.Current.Navigation.PushModalAsync(new FullScreenImagePage(picture, Complaint.Album.Id));
        }

        public async Task LoadDataAsync()
        {
            if (Complaint.Album.Id == 0)
                return;

            try
            {
                // Запрос к API для получения списка картинок
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.GetStringAsync(new API_URL($"/albums/{Complaint.Album.Id}/pictures"));

                // Десериализация ответа в объект PicturesResponse
                var responseObject = JsonConvert.DeserializeObject<PicturesResponse>(response);
                if (responseObject?.Pictures == null)
                    return;

                Debug.WriteLine($"Ответ от API: {response}");

                AlbumPictures.Clear();

                // Генерация пути для картинок
                foreach (var picture in responseObject.Pictures)
                {
                    picture.Path ??= new API_URL($"/albums/{Complaint.Album.Id}/pictures/{picture.Id}/thumb/q480?sign={responseObject.Sign}");
                    picture.OriginalPath ??= new API_URL($"/albums/{Complaint.Album.Id}/pictures/{picture.Id}/original?sign={responseObject.Sign}");
                }

                // Если есть указанная картинка, делаем её первой
                if (!string.IsNullOrEmpty(Complaint.Picture?.Path))
                {
                    var mainPicture = responseObject.Pictures.FirstOrDefault(p => p.Name == Complaint.Picture.Path);
                    if (mainPicture != null)
                    {
                        AlbumPictures.Add(mainPicture);
                        responseObject.Pictures.Remove(mainPicture);

                        MainImagePath = mainPicture.Path;
                        IsMainImageVisible = true;
                    }
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
    }
}

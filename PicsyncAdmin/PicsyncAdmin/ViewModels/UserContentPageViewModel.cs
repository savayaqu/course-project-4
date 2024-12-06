using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Resources;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PicsyncAdmin.ViewModels
{
    public partial class UserContentPageViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly string? _token = AuthSession.Token;

        public UserContentPageViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [ObservableProperty]
        private ulong albumId;

        [ObservableProperty]
        private string? picturePath;

        [ObservableProperty]
        private ObservableCollection<Models.Response.PictureComplaint> albumPictures = new();

        [ObservableProperty]
        private string? mainImagePath;

        [ObservableProperty]
        private bool isMainImageVisible;

        public async Task LoadDataAsync()
        {
            if (AlbumId == 0)
                return;

            try
            {
                // Запрос к API для получения списка картинок
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.GetStringAsync(new API_URL($"/albums/{AlbumId}/pictures"));

                // Десериализация ответа в объект PicturesResponse
                var responseObject = JsonConvert.DeserializeObject<PicturesResponse>(response);
                if (responseObject?.Pictures == null)
                    return;

                Debug.WriteLine($"Ответ от API: {response}");

                AlbumPictures.Clear();

                // Генерация пути для картинок
                foreach (var picture in responseObject.Pictures)
                {
                    picture.Path ??= new API_URL($"/albums/{AlbumId}/pictures/{picture.Id}/thumb/q480?sign={responseObject.Sign}");
                }

                // Если есть указанная картинка, делаем её первой
                if (!string.IsNullOrEmpty(PicturePath))
                {
                    var mainPicture = responseObject.Pictures.FirstOrDefault(p => p.Name == PicturePath);
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

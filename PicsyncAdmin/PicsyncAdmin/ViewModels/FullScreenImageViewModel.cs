using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;

namespace PicsyncAdmin.ViewModels
{
    public partial class FullScreenImageViewModel : ObservableObject
    {
        [ObservableProperty]
        private Picture picture;
        private readonly HttpClient _httpClient;
        private readonly string? _token = AuthSession.Token;

        [ObservableProperty]
        private ulong albumId;
        public FullScreenImageViewModel(Picture picture,ulong albumId)
        {
            _httpClient = new HttpClient();
            AlbumId = albumId;
            Picture = picture;
        }

        [RelayCommand]
        public async Task Close()
        {
            await Shell.Current.Navigation.PopModalAsync(); // Закрываем модальное окно
        }
        [RelayCommand]
        public async Task DeleteImage(Picture picture)
        {
            var confirmDelete = await Shell.Current.DisplayAlert("Подтверждение", "Вы уверены, что хотите удалить это изображение?", "Да", "Нет");

            if (confirmDelete)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                // Запрос к API для удаления изображения
                var response = await _httpClient.DeleteAsync(new API_URL($"/albums/{AlbumId}/pictures/{picture.Id}"));

                if (response.IsSuccessStatusCode)
                {
                    // Обрабатываем успешное удаление
                    await Shell.Current.DisplayAlert("Удаление", "Изображение удалено.", "Ок");
                    UserContentViewModel.Instance.AlbumPictures.Remove(picture);
                    await Shell.Current.Navigation.PopModalAsync(); // Закрываем модальное окно
                }
                else
                {
                    // Обрабатываем ошибку
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить изображение.", "Ок");
                }
            }
        }
    }
}

using System.Diagnostics;
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

        [ObservableProperty]
        private ulong albumId;
        public FullScreenImageViewModel(Picture picture, ulong albumId)
        {
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

            if (!confirmDelete) return;

            var response = await Fetch.DoAsync(
                HttpMethod.Delete,
                $"/albums/{AlbumId}/pictures/{picture.Id}",
                setError: msg => Debug.WriteLine(msg)
            );

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Удаление", "Изображение удалено.", "Ок");
                UserContentViewModel.Instance.AlbumPictures.Remove(picture);
                await Shell.Current.Navigation.PopModalAsync(); // Закрываем модальное окно
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить изображение.", "Ок");
            }
        }
    }
}

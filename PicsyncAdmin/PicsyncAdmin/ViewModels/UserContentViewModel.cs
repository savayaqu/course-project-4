using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using System.Diagnostics;

namespace PicsyncAdmin.ViewModels
{
    //TODO: в разметке расположить красиво кнопки и инфу
    public partial class UserContentViewModel : ObservableObject
    {
        public static UserContentViewModel? Instance { get; private set; }

        private readonly string? _token = AuthSession.Token;
        public Album Album { get; set; }

        public UserContentViewModel(Album album)
        {
            Instance = this;
            Album = album;
            _ = LoadData();
        }

        [ObservableProperty]
        private ObservableCollection<Picture> albumPictures = new();

        [ObservableProperty]
        private bool isFullScreenVisible;

        [ObservableProperty]
        private bool areControlsVisible = true;
        [ObservableProperty]
        private Picture? currentPicture;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextImageCommand))]
        [NotifyCanExecuteChangedFor(nameof(PreviousImageCommand))]
        private int currentIndex;
        private bool CanLoadData() => !IsFetch;

        public bool CanGoToPrevious() => CurrentIndex > 0;
        public bool CanGoToNext() => CurrentIndex < PicturesCount - 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
        private bool isFetch = false;

        [ObservableProperty]
        private string statusMessage;

        [ObservableProperty]
        private bool canLoadMore = false;

        [ObservableProperty]
        private int currentPage = 1;
        [ObservableProperty]
        private int complaintCount;

        [ObservableProperty]
        private string? mainImagePath;

        [ObservableProperty]
        private bool isMainImageVisible;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextImageCommand))]
        private int picturesCount;

        [ObservableProperty]
        private int complaintsCount;

        [ObservableProperty]
        private string albumName;

        [ObservableProperty]
        private string userName;

        [RelayCommand]
        public async Task IssueWarning()
        {
            string comment = await Shell.Current.DisplayPromptAsync("Создание предупреждения", "Комментарий");

            var warningResponse = await Fetch.DoAsync(HttpMethod.Post, $"/users/{Album.User.Id}/warnings", setError: msg => Debug.WriteLine(msg), body: new { comment = comment }, serialize: true);

            var complaintResponse = await Fetch.DoAsync(HttpMethod.Post, $"/complaints/{Album.User.Id}", setError: msg => Debug.WriteLine(msg), body: new { status = 1 }, serialize: true);

            if (warningResponse.IsSuccessStatusCode && complaintResponse.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Успех", "Предупреждение выдано", "OK");
                await HomeViewModel.Instance.ResetComplaints();
                await Shell.Current.Navigation.PopModalAsync();
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

            if (!result) return;

            var response = await Fetch.DoAsync(HttpMethod.Delete, $"/albums/{Album.Id}", setError: msg => Debug.WriteLine(msg));

            if (response.IsSuccessStatusCode)
            {
                await LoadData();
                await Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить альбом", "OK");
            }
        }

        [RelayCommand]
        public async Task BlockUser()
        {
            var result = await Shell.Current.DisplayAlert(
                "Подтверждение блокировки",
                "Вы уверены, что хотите заблокировать этого пользователя?",
                "Да", "Нет");

            if (!result) return;

            var response = await Fetch.DoAsync(HttpMethod.Post, $"/users/{Album.User.Id}", setError: msg => Debug.WriteLine(msg), body: new { is_banned = true }, serialize: true);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Успех", "Пользователь заблокирован", "OK");
                await HomeViewModel.Instance.ResetComplaints();
                await Shell.Current.Navigation.PopModalAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось заблокировать пользователя", "OK");
            }
        }

        [RelayCommand]
        public async Task RejectComplaint()
        {
            var response = await Fetch.DoAsync(HttpMethod.Delete, $"/complaints/{Album.User.Id}", setError: msg => Debug.WriteLine(msg));

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось отклонить жалобу", "OK");
            }
        }

        [RelayCommand]
        private void ViewImage(Picture picture)
        {
            CurrentPicture = picture;
            CurrentIndex = AlbumPictures.IndexOf(picture);
            IsFullScreenVisible = true;
            Debug.WriteLine(picture.ComplaintCount);
            Shell.SetNavBarIsVisible(Shell.Current, false);
            Shell.SetTabBarIsVisible(Shell.Current.CurrentPage, false);
        }

        [RelayCommand]
        private void CloseFullScreen()
        {
            IsFullScreenVisible = false;
            CurrentPicture = null;
            Shell.SetNavBarIsVisible(Shell.Current, true);
            Shell.SetTabBarIsVisible(Shell.Current, true);
        }

        [RelayCommand]
        public void ToggleControlsVisibility()
        {
            AreControlsVisible = !AreControlsVisible;

            // Платформо-специфичное управление статус-баром и навигационной панелью
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity != null)
            {
                if (AreControlsVisible)
                {
                    // Показать статус-бар и навигационную панель
                    activity.Window.DecorView.SystemUiVisibility = Android.Views.StatusBarVisibility.Visible;
                }
                else
                {
                    // Скрыть статус-бар и навигационную панель
                    activity.Window.DecorView.SystemUiVisibility =
                        (Android.Views.StatusBarVisibility)(
                            Android.Views.SystemUiFlags.HideNavigation |
                            Android.Views.SystemUiFlags.Fullscreen |
                            Android.Views.SystemUiFlags.ImmersiveSticky
                        );
                }
            }
#elif IOS
    UIKit.UIApplication.SharedApplication.SetStatusBarHidden(!AreControlsVisible, UIKit.UIStatusBarAnimation.Fade);
#endif
        }



        [RelayCommand]
        public async Task DeleteImage()
        {
            var confirmDelete = await Shell.Current.DisplayAlert("Подтверждение", "Вы уверены, что хотите удалить это изображение?", "Да", "Нет");

            if (!confirmDelete) return;

            var response = await Fetch.DoAsync(
                HttpMethod.Delete,
                $"/albums/{Album.Id}/pictures/{CurrentPicture.Id}",
                setError: msg => Debug.WriteLine(msg)
            );

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Удаление", "Изображение удалено.", "Ок");
                AlbumPictures.Remove(CurrentPicture);

                // Обновляем текущую картинку
                if (CanGoToNext()) // Если можно перейти к следующему изображению
                {
                    _ = NextImage();
                }
                else if (CanGoToPrevious()) // Если нет следующего, переходим к предыдущему
                {
                    PreviousImage();
                }
                else
                {
                    // Если не осталось изображений, возвращаем на главную страницу или выполняем другие действия
                    await Shell.Current.GoToAsync("//HomePage");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить изображение.", "Ок");
            }
        }


        [RelayCommand(CanExecute = nameof(CanGoToPrevious))]
        private void PreviousImage()
        {
            CurrentIndex--;
            CurrentPicture = AlbumPictures[CurrentIndex];
        }

        [RelayCommand(CanExecute = nameof(CanGoToNext))]
        private async Task NextImage()
        {
            try
            {
                // Проверяем, можно ли загрузить больше картинок
                if (CanGoToNext() && CanLoadMore && CurrentIndex == AlbumPictures.Count-1)
                {
                    await LoadData();  // Загружаем больше данных, если это возможно
                }
                // После загрузки новых картинок (или если их не было), переходим к следующей картинке
                if (CanGoToNext())
                {
                    CurrentIndex++;
                    CurrentPicture = AlbumPictures[CurrentIndex];
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Произошла ошибка при загрузке следующего изображения.", "OK");
            }
        }



        [RelayCommand(CanExecute = nameof(CanLoadData))]
        public async Task LoadData()
        {
            if (Album.Id == 0) return;

            try
            {
                StatusMessage = "Загрузка данных...";
                IsFetch = true;

                // Получение списка картинок
                var response = await Fetch.DoAsync(HttpMethod.Get, $"/albums/{Album.Id}/pictures?reverse&sort=complaints&page={CurrentPage}", setError: msg => Debug.WriteLine(msg));
                // Получение информации об альбоме
                var responseAlbum = await Fetch.DoAsync(HttpMethod.Get, $"/albums/{Album.Id}", setError: msg => Debug.WriteLine(msg));
                IsFetch = false;

                if (!response.IsSuccessStatusCode || !responseAlbum.IsSuccessStatusCode)
                {
                    StatusMessage = "Ошибка загрузки данных. Попробуйте снова.";
                    return;
                }

                // Разбор ответа с картинками
                var responseObject = JsonConvert.DeserializeObject<PicturesResponse>(await response.Content.ReadAsStringAsync());
                if (responseObject?.Pictures == null || !responseObject.Pictures.Any())
                {
                    StatusMessage = "Картинки отсутствуют.";
                    return;
                }

                // Разбор ответа с данными альбома
                var albumData = JsonConvert.DeserializeObject<AlbumResponse>(await responseAlbum.Content.ReadAsStringAsync());
                if (albumData?.Album == null)
                {
                    StatusMessage = "Не удалось загрузить данные альбома.";
                    return;
                }

                // Запись данных в свойства
                PicturesCount = albumData.Album.PicturesCount;
                ComplaintsCount = albumData.Album.ComplaintsCount;
                AlbumName = albumData.Album.Name;
                UserName = albumData.Album.User?.Name ?? "";
                StatusMessage = ""; // Скрываем статус, если данные загружены

                // Логика работы с картинками
                CanLoadMore = responseObject.Total > responseObject.Page * responseObject.Limit;
                CurrentPage++;

                foreach (var picture in responseObject.Pictures)
                {
                    picture.OriginalPath ??= new API_URL($"/albums/{Album.Id}/pictures/{picture.Id}/original?sign={responseObject.Sign}");
                    picture.Path ??= new API_URL($"/albums/{Album.Id}/pictures/{picture.Id}/thumb/q480?sign={responseObject.Sign}");
                }

                foreach (var picture in responseObject.Pictures)
                {
                    AlbumPictures.Add(picture);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                StatusMessage = "Произошла ошибка при загрузке данных.";
            }
        }
    }
}

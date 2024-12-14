using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PicsyncAdmin.ViewModels
{
    public partial class UserContentViewModel : ObservableObject
    {
        public static UserContentViewModel? Instance { get; private set; }

        private readonly string? _token = AuthSession.Token;
        public Album Album { get; set; }

        public UserContentViewModel(Album album)
        {
            Instance = this;
            Album = album;
            _= LoadData();
        }

        [ObservableProperty]
        private ObservableCollection<Picture> albumPictures = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
        private bool isFetch = false;

        [ObservableProperty]
        private bool canLoadMore = false;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private string? mainImagePath;

        [ObservableProperty]
        private bool isMainImageVisible;

        [RelayCommand]
        public async Task IssueWarning()
        {
            string comment = await Shell.Current.DisplayPromptAsync("Создание предупреждения", "Комментарий");

            var warningResponse = await Fetch.DoAsync(HttpMethod.Post, $"/users/{Album.User.Id}/warnings", setError: msg => Debug.WriteLine(msg), body: new { Comment = comment }, serialize: true);

            var complaintResponse = await Fetch.DoAsync(HttpMethod.Post, $"/complaints/{Album.User.Id}", setError: msg => Debug.WriteLine(msg), body: new { Status = 1 }, serialize: true);

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
        public async Task ViewImage(Picture picture)
        {
            await Shell.Current.Navigation.PushModalAsync(new FullScreenImagePage(picture, Album.Id));
        }

        [RelayCommand(CanExecute = nameof(CanLoadData))]
        public async Task LoadData()
        {
            if (Album.Id == 0) return;

            try
            {
                IsFetch = true;

                var response = await Fetch.DoAsync(HttpMethod.Get, $"/albums/{Album.Id}/pictures?page={CurrentPage}", setError: msg => Debug.WriteLine(msg));

                IsFetch = false;

                if (!response.IsSuccessStatusCode) return;

                var responseObject = JsonConvert.DeserializeObject<PicturesResponse>(await response.Content.ReadAsStringAsync());

                if (responseObject?.Pictures == null) return;

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
                Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private bool CanLoadData() => !IsFetch;
    }
}

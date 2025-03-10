using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using CommunityToolkit.Maui.Views;
using PicsyncClient.Components.Popups;
using PicsyncClient.Utils;
#if ANDROID
using Android.Views;
#endif

namespace PicsyncClient.ViewModels;

public partial class ViewerViewModel : ObservableObject
{
    public AlbumViewModel? AlbumViewModel { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveNextCommand))]
    [NotifyCanExecuteChangedFor(nameof(MovePreviousCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayedPosition))]
    [NotifyCanExecuteChangedFor(nameof(MoveNextCommand))]
    [NotifyCanExecuteChangedFor(nameof(MovePreviousCommand))]
    private int? _position;

    public int? DisplayedPosition => Position + 1;

    [ObservableProperty]
    private int? _total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRemote))]
    [NotifyPropertyChangedFor(nameof(IsSynced))]
    [NotifyPropertyChangedFor(nameof(IsNotSynced))]
    [NotifyPropertyChangedFor(nameof(IsUnique))]
    private Models.Pictures.IPicture _picture;

    public ViewerViewModel(Models.Pictures.IPicture picture, AlbumViewModel? albumViewModel = null)
    {
        AlbumViewModel = albumViewModel;
        Picture = picture;

        if (AlbumViewModel == null || AlbumViewModel.PicturesGroups.Count < 1) return;

        Total = AlbumViewModel.Album.PicturesCount;
        Position = 0;
        foreach (var group in AlbumViewModel.PicturesGroups)
        {
            foreach (var pictureInGroup in group)
            {
                if (pictureInGroup == Picture || pictureInGroup.Equals(Picture))
                {
                    return;
                }
                Position++;
            }
        }
    }

    public bool IsAlbumLocal       => Picture.Album is IAlbumLocal;
    public bool IsAlbumSynced      => Picture.Album is AlbumSynced;
    public bool IsAlbumRemote      => Picture.Album is AlbumRemote;
    public bool IsAlbumNonOwned    => Picture.Album is AlbumRemote { Owner: not null };
    public bool IsAlbumRemoteOwned => Picture.Album is AlbumRemote { Owner: null };
    
    public bool IsRemote    => Picture is PictureRemote;
    public bool IsSynced    => IsAlbumSynced && Picture is PictureSynced;
    public bool IsNotSynced => IsAlbumSynced && Picture is PictureLocal;
    public bool IsUnique    => IsAlbumSynced && Picture.GetType() == typeof(PictureRemote);

    [ObservableProperty]
    private bool _areControlsVisible = true;

    [RelayCommand]
    private void ToggleControlsVisibility()
    {
        AreControlsVisible = !AreControlsVisible;

#if ANDROID
        // Платформо-специфичное управление статус-баром и навигационной панелью 
        var activity = Platform.CurrentActivity;
        System.Diagnostics.Debug.WriteLine($"activity: {activity}");
        if (activity == null 
         || activity.Window == null 
         || activity.Window.DecorView == null) return;
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
#elif IOS
        UIKit.UIApplication.SharedApplication.SetStatusBarHidden(!AreControlsVisible, UIKit.UIStatusBarAnimation.Fade);
#endif
    }


    [RelayCommand]
    private async Task OpenInfo()
    {
        PictureInfoPopup popup = new(Picture);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is bool and true)
        {
            if (Picture is PictureRemote remote)
                RemoteAlbumsData.AlbumsAccessible.Remove(remote.SpecificAlbum);

            _ = Shell.Current.GoToAsync("//Albums");
        }

        OnPropertyChanged(nameof(Picture));
    }

    private bool CanMoveNext() => AlbumViewModel != null 
        && Total > 0
        && Position < Total - 1
        && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanMoveNext))]
    private async Task MoveNext()
    {
        if (AlbumViewModel == null || Position == null || Total == null)
            return;

        IsBusy = true;

        Position++;

        if (Position >= AlbumViewModel.PicturesGroups.Sum(g => g.Count))
        {
            await AlbumViewModel.LoadMoreCommand.ExecuteAsync(null);
        }

        UpdateCurrentPicture();

        IsBusy = false;
    }

    private bool CanMovePrevious() => AlbumViewModel != null 
        && Total > 0
        && Position > 0
        && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanMovePrevious))]
    private async Task MovePrevious()
    {
        if (AlbumViewModel == null || Position == null || Total == null)
            return;

        IsBusy = true;

        Position--;

        if (Position < 0)
        {
            Position = Total.Value - 1;
        }

        UpdateCurrentPicture();

        IsBusy = false;
    }


    private void UpdateCurrentPicture()
    {
        if (AlbumViewModel == null || Position == null)
            return;

        int index = 0;
        foreach (var group in AlbumViewModel.PicturesGroups)
        {
            foreach (var pictureInGroup in group)
            {
                if (index == Position)
                {
                    Picture = pictureInGroup;
                    return;
                }
                index++;
            }
        }

        // Если картинка не найдена, сбрасываем Position и Picture
        Position = null;
    }
}

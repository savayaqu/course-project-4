using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using CommunityToolkit.Maui.Views;
using PicsyncClient.Components.Popups;
using PicsyncClient.Utils;



#if ANDROID
using Android.OS;
using Android.Views;
#endif

namespace PicsyncClient.ViewModels;

public partial class ViewerMainViewModel : ObservableObject
{
    public List<Models.Pictures.IPicture>? ListPictures { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAlbumSynced))]
    [NotifyPropertyChangedFor(nameof(IsRemote))]
    [NotifyPropertyChangedFor(nameof(IsSynced))]
    [NotifyPropertyChangedFor(nameof(IsNotSynced))]
    [NotifyPropertyChangedFor(nameof(IsUnique))]
    private Models.Pictures.IPicture picture;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayedPosition))]
    [NotifyCanExecuteChangedFor(nameof(MoveNextCommand))]
    [NotifyCanExecuteChangedFor(nameof(MovePreviousCommand))]
    private int? position;

    public int? DisplayedPosition => (Position != null) ? (Position + 1) : null;

    public ViewerMainViewModel(
        Models.Pictures.IPicture picture,
        List<Models.Pictures.IPicture>? listPictures
    ) {
        Picture = picture;
        if (listPictures == null || 
            listPictures.Count < 2) return;

        Position = listPictures.IndexOf(picture);
        if (position == -1)
        {
            listPictures.Insert(0, picture);
            Position = 0;
        }
        ListPictures = listPictures;
    }

    public bool IsAlbumSynced      => Picture?.Album is AlbumSynced;
    //public bool IsAlbumLocal       => Picture?.Album is IAlbumLocal;
    //public bool IsAlbumRemote      => Picture?.Album is AlbumRemote;
    //public bool IsAlbumNonOwned    => Picture?.Album is AlbumRemote album && album.Owner != null;
    //public bool IsAlbumRemoteOwned => Picture?.Album is AlbumRemote album && album.Owner == null;

    public bool IsRemote    => Picture is PictureRemote;
    public bool IsSynced    => IsAlbumSynced && Picture is PictureSynced;
    public bool IsNotSynced => IsAlbumSynced && Picture is PictureLocal;
    public bool IsUnique    => IsAlbumSynced && Picture?.GetType() == typeof(PictureRemote);

    [ObservableProperty]
    private bool areControlsVisible = true;

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
    public async Task OpenInfo()
    {
        PictureInfoPopup popup = new(Picture);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is bool isUnjoin && isUnjoin) {
            if (Picture is PictureRemote remote)
            RemoteAlbumsData.AlbumsAccessible.Remove(remote.SpecificAlbum);
            _ = Shell.Current.GoToAsync("//Main");
        }

        OnPropertyChanged(nameof(Picture));
    }

    public bool CanMove => Position != null 
                         && ListPictures != null 
                         && ListPictures.Count > 1;

    [RelayCommand(CanExecute = nameof(CanMove))]
    private void MoveNext()
    {
        if (!CanMove) return;

        if (Position < ListPictures.Count - 1)
            Position++;
        else
            Position = 0;

        UpdateCurrentPicture();
    }

    [RelayCommand(CanExecute = nameof(CanMove))]
    private void MovePrevious()
    {
        if (!CanMove) return;

        if (Position > 0)
            Position--;
        else
            Position = ListPictures.Count - 1;

        UpdateCurrentPicture();
    }


    private void UpdateCurrentPicture()
    {
        if (ListPictures == null || Position == null) return;
        try
        {
            Picture = ListPictures[(int)Position];
        }
        catch (Exception ex) 
        {
#if DEBUG
            Shell.Current.DisplayAlert("DEBUG CATCH", ex.Message, "OK");
#endif
        }
    }
}

using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Response;
using PicsyncClient.Models.Request;
using PicsyncClient.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;
using static PicsyncClient.Utils.LocalDB;
using PicsyncClient.Components.Popups;
using PicsyncClient.Models.Pictures;
using PicsyncClient.Models;

namespace PicsyncClient.ViewModels.Popups;

public partial class AlbumSyncManagePopupViewModel : ObservableObject
{
    [ObservableProperty] private string? error;
    [ObservableProperty] private AlbumSynced   album;
    [ObservableProperty] private UploadsAlbum? uploadsObject = null;

    private readonly Popup _popup;

    public AlbumSyncManagePopupViewModel(Popup popup, AlbumSynced album)
    {
        _popup = popup;
        Album = album;
        UploadsObject = PictureSender.Default.UploadsAlbums.Where(up => up.Album == Album).FirstOrDefault();

        PictureSender.Default.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "IsUploading")
                OnPropertyChanged(nameof(IsUploading));
        };
    }

    public bool IsUploading => PictureSender.Default.IsUploading;

    [RelayCommand]
    public async Task Desync()
    {
        if (IsUploading)
            PictureSender.Default.StopUpload();

        if (UploadsObject != null)
        {
            PictureSender.Default.UploadsAlbums.Remove(UploadsObject);
            UploadsObject = null;
        }

        AlbumDesyncPopup popup = new(Album);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not AlbumLocal local) return;

        _popup.Close(local);
    }

    [RelayCommand]
    public void StartManualSync()
    {
        UploadsObject = PictureSender.Default.ManualAlbumUpload(Album);
        PictureSender.Default.StartUploadIfNotActive();
    }

    [RelayCommand]
    public void StopOrCancelUpload()
    {
        if (IsUploading)
        {
            PictureSender.Default.StopUpload();
            return;
        }

        if (UploadsObject != null)
        {
            PictureSender.Default.UploadsAlbums.Remove(UploadsObject);
            UploadsObject = null;
        }
    }

    [RelayCommand]
    public void Close()
    {
        _popup.Close();
    }
}

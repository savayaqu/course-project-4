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
using PicsyncClient.Models.Pictures;
using Microsoft.Maui.Graphics;

namespace PicsyncClient.ViewModels.Popups;

public partial class AlbumDesyncPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private bool    isRemoveRemote = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private AlbumSynced albumForDesync;

    private CancellationTokenSource _cancelTokenSource = new();
    private readonly Popup _popup;

    public AlbumDesyncPopupViewModel(Popup popup, AlbumSynced album)
    {
        _popup = popup;
        AlbumForDesync = album;
    }

    [RelayCommand]
    public void SwitchRemoveRemote()
    {
        IsRemoveRemote = !IsRemoveRemote;
    }

    public bool CanConfirm => !IsBusy;

    [RelayCommand]
    public async void Confirm()
    {
        if (IsRemoveRemote)
        {
            var res = await FetchAsync(
                HttpMethod.Delete, URLs.AlbumInfo(AlbumForDesync.Id),
                f => IsBusy = f, e => Error = e,
                cancellationToken: _cancelTokenSource.Token
            );

            if (Error != null) return;
        }

        var syncedPictures = AlbumForDesync.LocalPictures.OfType<PictureSynced>().ToList();

        foreach (var syncedPicture in syncedPictures)
        {
            PictureLocal localPicture = new(syncedPicture);

            DB.Delete(syncedPicture);

            AlbumForDesync.LocalPictures.ReplaceOrAdd(syncedPicture, localPicture);

            LocalData.Pictures.ReplaceOrAdd(syncedPicture, localPicture);
        }

        DB.Delete(AlbumForDesync);
        LocalData.Albums.Remove(AlbumForDesync);
        Debug.WriteLine($"AlbumDesyncPopupViewModel: DB.Delete: {AlbumForDesync.Id}");
        AlbumLocal local = new(AlbumForDesync);
        _popup.Close(local);
    }

    [RelayCommand]
    public void Cancel()
    {
        _cancelTokenSource.Cancel();
        _popup.Close();
    }
}

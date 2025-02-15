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

namespace PicsyncClient.ViewModels.Popups;

public partial class AlbumUnjoinPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private AlbumRemote album;

    private CancellationTokenSource _cancelTokenSource = new();
    private readonly Popup _popup;

    public AlbumUnjoinPopupViewModel(Popup popup, AlbumRemote album)
    {
        _popup = popup;
        Album = album;
    }

    public bool CanConfirm => !IsBusy;

    [RelayCommand]
    public async Task Confirm(CancellationToken token = default)
    {
        // TODO создание жалобы

        HttpResponseMessage? res = await FetchAsync(
            HttpMethod.Delete, 
            URLs.AlbumAccess(Album.Id),
            f => IsBusy = f, 
            e => Error = e,
            cancellationToken: token
        );

        if (res == null || !res.IsSuccessStatusCode) return;

        _popup.Close(true);
    }

    [RelayCommand]
    public void Cancel()
    {
        _cancelTokenSource.Cancel();
        _popup.Close();
    }
}

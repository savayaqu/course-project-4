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

namespace PicsyncClient.ViewModels.Popups;

public partial class AlbumSyncManagePopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private bool    albumsListIsVisible = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private string  albumNameDefault = "";
    [ObservableProperty] private string  albumNameNew = "";
    [ObservableProperty] private AlbumSynced album;
    [ObservableProperty] private ObservableCollection<AlbumRemote> albumsRemoteOwn = [];

    private readonly Popup _popup;

    public AlbumSyncManagePopupViewModel(Popup popup, AlbumSynced album)
    {
        _popup = popup;
        Album = album;
    }

    [RelayCommand]
    public async Task Desync()
    {
        AlbumDesyncPopup popup = new(Album);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not AlbumLocal local) return;

        _popup.Close(local);
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}

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

public partial class AlbumInfoPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private IAlbum  album;

    private readonly Popup _popup;

    public AlbumInfoPopupViewModel(Popup popup, IAlbum album)
    {
        _popup = popup;
        Album = album;
    }

    public bool IsLocal       => Album is IAlbumLocal;
    public bool IsSynced      => Album is AlbumSynced;
    public bool IsRemote      => Album is AlbumRemote;
    public bool IsNonOwned    => Album is AlbumRemote album && album.Owner != null;
    public bool IsRemoteOwned => Album is AlbumRemote album && album.Owner == null;


    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}

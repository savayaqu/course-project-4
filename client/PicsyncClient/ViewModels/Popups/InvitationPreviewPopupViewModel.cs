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

namespace PicsyncClient.ViewModels.Popups;

public partial class InvitationPreviewPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private string  code = "";
    [ObservableProperty] private AlbumRemote?        album;
    [ObservableProperty] private List<PictureRemote> pictures = [];

    private readonly Popup _popup;

    public InvitationPreviewPopupViewModel(Popup popup)
    {
        _popup = popup;
    }

    [RelayCommand]
    public async Task Paste()
    {
        Code = await Clipboard.Default.GetTextAsync();
        CheckCommand.Execute(null);
    }

    [RelayCommand]
    public async Task Check(CancellationToken token = default)
    {

        (var res, var body) = await FetchAsync<InvitationAlbumResponse>(
            HttpMethod.Get, URLs.InvitationAlbum(Code),
            f => IsBusy = f, e => Error = e,
            cancellationToken: token
        );
    }

    public bool CanConfirm => !IsBusy;

    [RelayCommand]
    public async void Join(CancellationToken token = default)
    {
        HttpResponseMessage res = await FetchAsync(
            HttpMethod.Post, URLs.InvitationJoin(Code),
            f => IsBusy = f, e => Error = e,
            cancellationToken: token
        );

        if (!res.IsSuccessStatusCode || Album is null) return;

        _popup.Close(Album);
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}

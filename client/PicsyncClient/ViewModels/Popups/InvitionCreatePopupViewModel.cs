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

public partial class InvitationCreatePopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private AlbumRemote album;
    [ObservableProperty] private int? joinLimit;
    [ObservableProperty] private DateTime? expiresAt;

    private CancellationTokenSource _cancelTokenSource = new();
    private readonly Popup _popup;

    public InvitationCreatePopupViewModel(Popup popup, AlbumRemote album)
    {
        _popup = popup;
        Album = album;
    }

    public bool CanConfirm => !IsBusy;

    [RelayCommand]
    public async void Confirm()
    {
        if (JoinLimit == 0) JoinLimit = null;

        (var res, var body) = await FetchAsync<InvitationResponse>(
            HttpMethod.Post, URLs.AlbumInvite(Album.Id),
            f => IsBusy = f, e => Error = e,
            new InvationCreateRequest(JoinLimit, ExpiresAt),
            cancellationToken: _cancelTokenSource.Token
        );

        if (body == null) return;

        _popup.Close(body.Invitation);
    }

    [RelayCommand]
    public void Cancel()
    {
        _cancelTokenSource.Cancel();
        _popup.Close();
    }
}

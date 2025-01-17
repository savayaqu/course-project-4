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
using PicsyncClient.Models;

namespace PicsyncClient.ViewModels.Popups;

public partial class AlbumAccessManagePopupViewModel : ObservableObject
{
    [ObservableProperty] private bool?       isBusy;
    [ObservableProperty] private string?     error;
    [ObservableProperty] private AlbumRemote album;

    private readonly Popup _popup;

    public AlbumAccessManagePopupViewModel(Popup popup, AlbumRemote album)
    {
        _popup = popup;
        Album = album;
    }

    [RelayCommand]
    public async Task InvitationCreate()
    {
        InvitationCreatePopup popup = new(Album);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not Invitation invitation) return;

        Album.Invitations ??= [];
        Album.Invitations.Add(invitation);
    }

    [RelayCommand]
    public async Task InvitationCodeCopy(Invitation invitation)
    {
        await Clipboard.Default.SetTextAsync(invitation.Code);
    }

    [RelayCommand]
    public async Task InvitationRemove(Invitation invitation, CancellationToken token = default)
    {
        HttpResponseMessage res = await FetchAsync(
            HttpMethod.Delete, URLs.Invitation(invitation.Code),
            f => IsBusy = f, e => Error = e,
            cancellationToken: token
        );

        if (!res.IsSuccessStatusCode) return;

        Album.Invitations?.Remove(invitation);
    }

    [RelayCommand]
    public async Task AccessRemove(User user, CancellationToken token)
    {
        HttpResponseMessage res = await FetchAsync(
            HttpMethod.Delete, URLs.AlbumAccess(Album.Id, user.Id),
            f => IsBusy = f, e => Error = e,
            cancellationToken: token
        );

        if (!res.IsSuccessStatusCode) return;

        Album.GrantAccesses?.Remove(user);
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}

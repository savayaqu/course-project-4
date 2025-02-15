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
using PicsyncClient.Models;

namespace PicsyncClient.ViewModels.Popups;

public partial class ComplaintCreatePopupViewModel : ObservableObject
{
    [ObservableProperty] private string? error;
    [ObservableProperty] private AlbumRemote   album;
    [ObservableProperty] private PictureRemote picture;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string message = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))] 
    private ComplaintType? type;

    public List<ComplaintType> ComplaintTypes => ServerData.Settings?.ComplaintTypes ?? [];

    private readonly Popup _popup;

    public ComplaintCreatePopupViewModel(Popup popup, AlbumRemote album, PictureRemote? picture = null)
    {
        _popup = popup;
        Album = album;
        Picture = picture;
    }

    public bool CanConfirm => Type != null && Message.Length > 0;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public async Task Confirm()
    {
        if (Type == null || Message.Length < 1) return;

        ComplaintRequest req = new(Type, Message);

        Uri url = Picture != null
            ? URLs.ComplaintToPicture(Album.Id, Picture.Id)
            : URLs.ComplaintToAlbum(Album.Id);

        HttpResponseMessage ? res = await FetchAsync(
            HttpMethod.Post,
            url,
            setError: e => Error = e,
            body: req,
            serialize: true
        );

        if (res == null || !res.IsSuccessStatusCode) return;

        _popup.Close(true);
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}

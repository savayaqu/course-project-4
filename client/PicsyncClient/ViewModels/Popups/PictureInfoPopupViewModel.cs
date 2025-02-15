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
using PicsyncClient.Components.Popups;
using PicsyncClient.Views;

namespace PicsyncClient.ViewModels.Popups;

public partial class PictureInfoPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool     isBusy = false;
    [ObservableProperty] private string?  error;
    [ObservableProperty] private Models.Pictures.IPicture currentPicture;

    private readonly Popup _popup;

    public PictureInfoPopupViewModel(Popup popup, Models.Pictures.IPicture picture)
    {
        _popup = popup;
        CurrentPicture = picture;
    }

    [RelayCommand]
    public async Task UpdateName()
    {
        if (CurrentPicture is not PictureRemote remote || remote.IsRemoteNonOwned) return;

        var result = await Shell.Current.DisplayPromptAsync(
            "Изменение картинки", 
            "Введите новое название картинки на сервере", 
            maxLength: 255,
            initialValue: CurrentPicture.Name
        );
        if (result == null || result == CurrentPicture.Name) return;

        PictureUpdateRequest req = new(result);

        (var res, var body) = await FetchAsync<PictureResponse>(
            HttpMethod.Post,
            URLs.PictureInfo(remote.SpecificAlbum.Id, remote.Id),
            setError: e => Error = e,
            body: req,
            serialize: true
        );
        if (body == null) return;

        remote.Name = body.Picture.Name;
        OnPropertyChanged(nameof(CurrentPicture));
    }

    [RelayCommand]
    private async Task GoToAlbum()
    {
        await Shell.Current.Navigation.PushAsync(new AlbumPage(CurrentPicture.Album));
        _popup.Close();
    }

    [RelayCommand]
    public async Task Complaint()
    {
        if (CurrentPicture is not PictureRemote remote || remote.IsRemoteOwned) return;

        ComplaintCreatePopup popup = new(remote.SpecificAlbum, remote);
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
    }

    [RelayCommand]
    public async Task Remove()
    {
        if (CurrentPicture is not PictureRemote remote || remote.IsRemoteNonOwned) return;

        bool result = await Shell.Current.DisplayAlert(
            "Внимание!",
            "Вы действительно хотите удалить эту картинку? Это действие нельзя будет обратить.",
            "Удалить", "Отмена"
        );

        if (!result) return;

        HttpResponseMessage? res = await FetchAsync(
            HttpMethod.Delete,
            URLs.PictureInfo(remote.SpecificAlbum.Id, remote.Id),
            setError: e => Error = e
        );
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}

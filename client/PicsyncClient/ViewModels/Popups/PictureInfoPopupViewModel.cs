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
        if (CurrentPicture is not PictureRemote remote) return;
        if (CurrentPicture.IsRemoteNonOwned) return;

        var result = await Shell.Current.DisplayPromptAsync(
            "Изменение картинки", 
            "Обновить название картинки", 
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
    public void Cancel()
    {
        _popup.Close();
    }
}

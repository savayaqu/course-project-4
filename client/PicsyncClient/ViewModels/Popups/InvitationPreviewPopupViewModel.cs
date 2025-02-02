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
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using System.Threading;
using PicsyncClient.Views;

namespace PicsyncClient.ViewModels.Popups;

public partial class InvitationPreviewPopupViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinCommand))]
    private bool isBusy = false;

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
        string? clipboard = await Clipboard.Default.GetTextAsync();

        if (string.IsNullOrEmpty(clipboard))
        {
            _ = Toast.Make("Буфер пуст").Show();
            return;
        }

        Code = clipboard;
        CheckCommand.Execute(null);
    }

    [RelayCommand]
    public void Clear()
    {
        Code = "";
        Album = null;
        Error = null;
        Pictures.Clear();
    }

    [RelayCommand]
    public async Task Check(CancellationToken token = default)
    {
        Uri uri;
        try
        {
            uri = URLs.InvitationAlbum(Code);
        }
        catch
        {
            Error = "Неверный код приглашения";
            return;
        }

        (var res, var body) = await FetchAsync<InvitationAlbumResponse>(
            HttpMethod.Get, uri,
            f => IsBusy = f, e => Error = e,
            cancellationToken: token
        );

        if (body == null) return;

        Album = body.Album;

        foreach (var picture in body.Pictures) 
            picture.Album = Album;

        Pictures = body.Pictures;
    }

    public bool CanJoin => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public async Task Join(CancellationToken token = default)
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


    [ObservableProperty]
    private int columnCount = 1;

    [ObservableProperty]
    private double? requestColumnWidth = 100;

    [ObservableProperty]
    private double columnWidth = 100;

    [RelayCommand]
    public void CalculateColumnsWidth(double containerWidth)
    {
        if (RequestColumnWidth != null)
            ColumnCount = Math.Max((int)(containerWidth / RequestColumnWidth), 1);

        ColumnWidth = containerWidth / ColumnCount;
        Debug.WriteLine($"=== ChgColW ===\n{ColumnWidth} = {containerWidth} / {ColumnCount}");
    }


    //[RelayCommand]
    //private async void OpenViewer(Models.Pictures.IPicture picture)
    //{
    //    await Shell.Current.Navigation.PushAsync(new ViewerPage(picture, this), false);
    //}
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Enum;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Response;
using PicsyncClient.Utils;
using PicsyncClient.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.ViewModels;

public partial class AlbumsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AlbumRemote> albumsRemote   = [];
    [ObservableProperty] private ObservableCollection<AlbumLocal>  albumsLocal    = [];
    [ObservableProperty] private bool    isFetch        = false;
    [ObservableProperty] private bool    hasPermissions = false;
    [ObservableProperty] private string? error          = null;

    public AlbumsViewModel()
    {
        _ = RequestLocalAlbums();
        _ = RequestRemoteAlbums();
    }

    public async Task RequestLocalAlbums()
    {
        try
        {
            HasPermissions = await LocalData.CheckPermissions();
        }
        catch (PlatformNotSupportedException)
        {
            return;
        }

        if (HasPermissions)
        {
            if (LocalData.Status == LocalLoadStatus.NotLoad)
            {
                await LocalData.FillPictures();
            }
            else if (LocalData.Status == LocalLoadStatus.InLoad)
            {
                _ = Shell.Current.DisplayAlert("Страшилка", "TODO: непроверенный код", "OK");
                // TODO: непроверенный код
                await Task.Run(async () =>
                {
                    while (LocalData.Status == LocalLoadStatus.InLoad)
                    {
                        await Task.Delay(1000);
                    }
                });
            }
            AlbumsLocal = new(LocalData.Albums);
        }
    }

    [RelayCommand]
    private async Task RequestPermissions()
    {
        try
        {
            HasPermissions = await LocalData.RequestPermissions();
            if (HasPermissions)
            {
                _ = RequestLocalAlbums();
            }
            else
            {
                _ = Shell.Current.DisplayAlert("Ошибка", "Вы должны дать разрешение для просмотра локальных альбомов", "OK");
            }
        }
        catch (PlatformNotSupportedException ex)
        {
            _ = Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            return;
        }
    }

    public async Task RequestRemoteAlbums()
    {
        (var res, var body) = await FetchAsync<AlbumsResponse>(
            HttpMethod.Get, URLs.Albums,
            isFetch => IsFetch = isFetch,
            error   => Error   = error
        );
        if (body == null)
        {
            Debug.WriteLine($"RequestRemoteAlbums: body empty\n{JsonSerializer.Serialize(res)}");
            return;
        }

        AlbumsRemote = new(body.Own.Concat(body.Accessible));
        Debug.WriteLine(AlbumsRemote[0]?.ThumbnailPaths[0]);
    }

    [RelayCommand]
    private async Task GoToAlbum(IAlbum album)
    {
        await Shell.Current.Navigation.PushAsync(new AlbumPage(album));
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SquareWidth))]
    private int columnCount = 3;

    [ObservableProperty]
    private double? requestColumnWidth = 200;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SquareWidth))]
    private double columnWidth = 100;

    public double SquareWidth => ColumnWidth - 5 * (ColumnCount - 1) - 20;

    [RelayCommand]
    public void CalculateColumnsWidth(double containerWidth)
    {
        if (RequestColumnWidth != null)
            ColumnCount = Math.Max((int)(containerWidth / RequestColumnWidth), 1);

        ColumnWidth = containerWidth / ColumnCount;
        Debug.WriteLine($"=== ChgColW ===\n{ColumnWidth} = {containerWidth} / {ColumnCount}");
    }
}

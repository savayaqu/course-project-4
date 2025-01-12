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
using static PicsyncClient.Utils.LocalDB;

namespace PicsyncClient.ViewModels;

public partial class AlbumsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AlbumSynced> albumsSynced = [];
    [ObservableProperty] private ObservableCollection<AlbumRemote> albumsRemote = [];
    [ObservableProperty] private ObservableCollection<AlbumLocal>  albumsLocal  = [];

    public AlbumsViewModel()
    {
        AlbumsSynced.CollectionChanged += OnAlbumsSyncedCollectionChanged;

        _ = RequestAlbums();
    }

    public bool SyncedIsVisibled => AlbumsSynced.Count > 0;

    private void OnAlbumsSyncedCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(SyncedIsVisibled));

    partial void OnAlbumsSyncedChanged(ObservableCollection<AlbumSynced> oldValue, ObservableCollection<AlbumSynced> newValue)
    {
        if (oldValue != null)
            oldValue.CollectionChanged -= OnAlbumsSyncedCollectionChanged;

        if (newValue != null)
            newValue.CollectionChanged += OnAlbumsSyncedCollectionChanged;

        OnPropertyChanged(nameof(SyncedIsVisibled));
    }

    private async Task RequestAlbums()
    {
        var localTask  = RequestLocalAlbumsCommand.ExecuteAsync(null);
        var remoteTask = RequestRemoteAlbumsCommand.ExecuteAsync(null);

        await Task.WhenAll(localTask, remoteTask);

        //if (localTask .IsFaulted ||
        //    remoteTask.IsFaulted) return; // TODO: переделать/вынести

        if (Error != null) return;

        List<AlbumSynced> syncedAlbumsFromLocal = new(AlbumsSynced);
        Debug.WriteLine("RequestAlbums: AlbumsRemote: \n" + JsonSerializer.Serialize(AlbumsRemote));
        Debug.WriteLine("RequestAlbums: syncedAlbumsFromLocal: \n" + JsonSerializer.Serialize(syncedAlbumsFromLocal));
        AlbumsSynced.Clear();

        // Проверка что синхронизирующиеся альбомы до сих пор на сервере
        for (int i = AlbumsRemote.Count - 1; i >= 0; i--)
        {
            var remote = AlbumsRemote[i];
            var synced = syncedAlbumsFromLocal.FirstOrDefault(a => a.Id == remote.Id);

            Debug.WriteLine($"RequestAlbums: for(): AlbumsRemote[{i}] = \n" + JsonSerializer.Serialize(remote) + $"\n [[[synced]]] = \n{JsonSerializer.Serialize(synced)}");

            Debug.WriteLine($"RequestAlbums: synced == null: {synced == null}");
            if (synced is not AlbumSynced syncedTrue) continue;

            Debug.WriteLine($"RequestAlbums: synced.simple: #{syncedTrue.Id} {syncedTrue.LocalPath}");

            Debug.WriteLine($"RequestAlbums: synced.Update: {syncedTrue == null}");
            syncedTrue.Update(remote);

            Debug.WriteLine($"RequestAlbums: DB.Update: \n{JsonSerializer.Serialize(syncedTrue)}");
            DB.Update(syncedTrue);

            Debug.WriteLine($"RequestAlbums: AlbumsRemote.RemoveAt: {i}");
            AlbumsRemote.RemoveAt(i);

            Debug.WriteLine($"RequestAlbums: AlbumsSynced.Add");
            AlbumsSynced.Add(syncedTrue); 

            Debug.WriteLine($"RequestAlbums: syncedAlbumsFromLocal.Remove");
            syncedAlbumsFromLocal.Remove(syncedTrue);
        }

        Debug.WriteLine("RequestAlbums: syncedAlbumsFromLocal END: \n" + JsonSerializer.Serialize(syncedAlbumsFromLocal));
        // Оставшиейся в syncedAlbumsFromLocal более не синхронизируются
        foreach (var nonSynced in syncedAlbumsFromLocal)
        {
            Debug.WriteLine("RequestAlbums: foreach: \n" + JsonSerializer.Serialize(nonSynced));
            AlbumLocal localAlbum = new(nonSynced);

            AlbumsLocal.Add(localAlbum);

            LocalData.Albums.Remove(nonSynced);
            LocalData.Albums.Add(localAlbum);

            DB.Delete(nonSynced);
        }
    }


    [ObservableProperty] 
    private bool hasPermissions = false;

    [RelayCommand]
    private async Task RequestPermissions()
    {
        try
        {
            HasPermissions = await LocalData.RequestPermissions();
            if (HasPermissions)
            {
                _ = RequestLocalAlbumsCommand.ExecuteAsync(null);
            }
            else
            {
                _ = Shell.Current.DisplayAlert("Ошибка", "Вы должны дать разрешение на просмотр картинок", "OK");
            }
        }
        catch (PlatformNotSupportedException ex)
        {
            _ = Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestLocalAlbumsCommand))]
    public bool canUpdateLocal = false;

    [RelayCommand(CanExecute = nameof(CanUpdateLocal))]
    public async Task RequestLocalAlbums()
    {
        // Если не разрешено или не поддерживается — произойдёт исключение
        HasPermissions = await LocalData.CheckPermissions();
        if (!HasPermissions) 
            throw new UnauthorizedAccessException("Пользователь не выдавал разрешения на чтение");

        if (LocalData.Status == LocalLoadStatus.NotLoad)
        {
            await LocalData.FillPictures();
        }
        else if (LocalData.Status == LocalLoadStatus.InLoad)
        {
            // TODO: непроверенный код
            _ = Shell.Current.DisplayAlert("Страшилка", "TODO: непроверенный код", "OK");
            while (LocalData.Status == LocalLoadStatus.InLoad)
            {
                await Task.Delay(1000);
            }
        }
        CanUpdateLocal = true;
        AlbumsLocal  = new(LocalData.Albums.OfType<AlbumLocal >().ToList());
        AlbumsSynced = new(LocalData.Albums.OfType<AlbumSynced>().ToList());
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRequestRemote))]
    [NotifyCanExecuteChangedFor(nameof(RequestRemoteAlbumsCommand))]
    private bool isFetch = false;

    [ObservableProperty] 
    private string? error = null;

    public bool CanRequestRemote => !IsFetch;

    [RelayCommand(CanExecute = nameof(CanRequestRemote))]
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
            //throw new HttpRequestException("Нужный ответ не пришёл");
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
    private double? requestColumnWidth = 180;

    [ObservableProperty]
    private double squareWidth;

    [RelayCommand]
    public void CalculateColumnsWidth(double containerWidth)
    {
        if (RequestColumnWidth != null)
            ColumnCount = Math.Max((int)(containerWidth / RequestColumnWidth), 1);

        SquareWidth = ( (containerWidth - 5*(ColumnCount - 1)) / ColumnCount ) - 15;

        Debug.WriteLine($"=== SquareWidth ===\n{SquareWidth} = {containerWidth} / {ColumnCount}");
    }


    public void Reset()
    {
        AlbumsRemote.Clear();
        IsFetch = false;
        Error = null;
        RequestRemoteAlbumsCommand.Execute(null);
    }
}

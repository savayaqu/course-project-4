using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Components.Popups;
using PicsyncClient.Enum;
using PicsyncClient.Models.Albums;
using PicsyncClient.Utils;
using PicsyncClient.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using static PicsyncClient.Utils.LocalDB;

namespace PicsyncClient.ViewModels;

public partial class AlbumsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AlbumLocal>  _albumsLocal  = [];
    [ObservableProperty] private ObservableCollection<AlbumSynced> _albumsSynced = [];
    [ObservableProperty] private ObservableCollection<AlbumRemote> _albumsRemote = [];

    public AlbumsViewModel()
    {
        AlbumsSynced.CollectionChanged += OnAlbumsSyncedCollectionChanged;
        RequestAlbumsCommand.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(AsyncRelayCommand.IsRunning))
                OnPropertyChanged(nameof(CanRequestAlbums));
        };

        RequestAlbumsCommand.Execute(null);
    }

    public bool SyncedIsVisible => AlbumsSynced.Count > 0;

    private void OnAlbumsSyncedCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(SyncedIsVisible));

    partial void OnAlbumsSyncedChanged(ObservableCollection<AlbumSynced>? oldValue, ObservableCollection<AlbumSynced> newValue)
    {
        if (oldValue != null)
            oldValue.CollectionChanged -= OnAlbumsSyncedCollectionChanged;

        newValue.CollectionChanged += OnAlbumsSyncedCollectionChanged;

        OnPropertyChanged(nameof(SyncedIsVisible));
    }
    public bool CanRequestAlbums => !(RequestAlbumsCommand.IsRunning || IsFetch);

    [RelayCommand(CanExecute = nameof(CanRequestAlbums), IncludeCancelCommand = true)]
    private async Task<bool> RequestAlbums(CancellationToken token = default)
    {
        var remoteTask = RequestRemoteAlbumsCommand.ExecuteAsync(token);
        var localTask  = RequestLocalAlbumsCommand.ExecuteAsync(null);

        await Task.WhenAll(localTask, remoteTask);

        if (!HasPermissions || Error != null) return false;

        List<AlbumSynced> syncedAlbumsFromLocal = new(AlbumsSynced);
        AlbumsSynced.Clear();

        // Проверка, что синхронизирующиеся альбомы до сих пор на сервере
        for (int i = AlbumsRemote.Count - 1; i >= 0; i--)
        {
            var remote = AlbumsRemote[i];
            var synced = syncedAlbumsFromLocal.FirstOrDefault(a => a.Id == remote.Id);

            if (synced is not { }) continue;

            Debug.WriteLine($"RequestAlbums: synced.simple: #{synced.Id} {synced.LocalPath}");
            synced.Update(remote);

            DB.Update(synced);

            AlbumsRemote.RemoveAt(i);
            AlbumsSynced.Insert(0, synced); 

            syncedAlbumsFromLocal.Remove(synced);
        }
        
        // Оставшиеся в syncedAlbumsFromLocal более не синхронизируются
        foreach (var nonSynced in syncedAlbumsFromLocal)
        {
            Debug.WriteLine("RequestAlbums: foreach: \n" + JsonSerializer.Serialize(nonSynced));
            AlbumLocal localAlbum = new(nonSynced);

            AlbumsLocal.Add(localAlbum);

            LocalData.Albums.ReplaceOrAdd(nonSynced, localAlbum);

            DB.Delete(nonSynced);
        }
        return true;
    }


    [ObservableProperty] 
    private bool _hasPermissions;

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
    private bool _canUpdateLocal;

    [RelayCommand(CanExecute = nameof(CanUpdateLocal))]
    public async Task<bool> RequestLocalAlbums()
    {
        // Если не разрешено или не поддерживается — произойдёт исключение
        try
        {
            HasPermissions = await LocalData.CheckPermissions();
            if (!HasPermissions)
                return false;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }

        if (LocalData.Status == LocalLoadStatus.NotLoad)
        {
            await LocalData.FillPictures();
        }
        else if (LocalData.Status == LocalLoadStatus.InLoad)
        {
            while (LocalData.Status == LocalLoadStatus.InLoad)
            {
                await Task.Delay(1000);
            }
        }
        else if (LocalData.Status == LocalLoadStatus.Loaded && CanUpdateLocal)
        {
            await LocalData.FillPictures();
        }
        CanUpdateLocal = true;
        AlbumsLocal .UpdateFrom(LocalData.Albums.OfType<AlbumLocal >().ToList());
        AlbumsSynced.UpdateFrom(LocalData.Albums.OfType<AlbumSynced>().ToList());
        return true;
    }

    [RelayCommand]
    public void LightUpdate()
    {
        AlbumsLocal .UpdateFrom(LocalData.Albums.OfType<AlbumLocal >().ToList());
        AlbumsSynced.UpdateFrom(LocalData.Albums.OfType<AlbumSynced>().ToList());

        var remotes = RemoteAlbumsData.AlbumsOwn.Concat(RemoteAlbumsData.AlbumsAccessible).ToList();

        foreach (var synced in AlbumsSynced)
        {
            remotes.RemoveAll(r => r.Id == synced.Id);
        }

        AlbumsRemote.UpdateFrom(remotes);
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRequestRemote))]
    [NotifyPropertyChangedFor(nameof(CanRequestAlbums))]
    [NotifyCanExecuteChangedFor(nameof(RequestAlbumsCommand))] 
    [NotifyCanExecuteChangedFor(nameof(RequestRemoteAlbumsCommand))]
    private bool _isFetch;

    [ObservableProperty] 
    private string? _error;

    public bool CanRequestRemote => !IsFetch;

    [RelayCommand(CanExecute = nameof(CanRequestRemote), IncludeCancelCommand = true)]
    public async Task RequestRemoteAlbums(CancellationToken token = default)
    {
        await RemoteAlbumsData.FillAlbums(
            setIsFetch: f => IsFetch = f,
            setError: e => Error = e,
            cancellationToken: token
        );

        AlbumsRemote.UpdateFrom( // TODO: отдельно обновлять только инфу
            RemoteAlbumsData.AlbumsOwn
            .Concat(RemoteAlbumsData.AlbumsAccessible)
            .ToList()
        );
    }


    [RelayCommand]
    private async Task GoToAlbum(IAlbum album)
    {
        await Shell.Current.Navigation.PushAsync(new AlbumPage(album));
    }


    [RelayCommand]
    private async Task OpenInvitationPreview()
    {
        InvitationPreviewPopup popup = new();
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not AlbumRemote remote) return;

        AlbumsRemote.Add(remote);
    }


    public void Reset()
    {
        AlbumsRemote.Clear();
        IsFetch = false;
        Error = null;
        RequestRemoteAlbumsCommand.Execute(null);
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SquareWidth))]
    private int _columnCount = 1;

    [ObservableProperty]
    private double? _requestColumnWidth = 180;

    [ObservableProperty]
    private double _squareWidth;

    [RelayCommand]
    private void CalculateColumnsWidth(double containerWidth)
    {
        if (RequestColumnWidth != null)
            ColumnCount = Math.Max((int)(containerWidth / RequestColumnWidth), 1);

        SquareWidth = ( (containerWidth - 5*(ColumnCount - 1)) / ColumnCount ) - 15;

        Debug.WriteLine($"=== SquareWidth ===\n{SquareWidth} = {containerWidth} / {ColumnCount}");
    }
}

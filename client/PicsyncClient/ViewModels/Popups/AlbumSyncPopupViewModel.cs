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

public partial class AlbumSyncPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private bool    albumsListIsVisible = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private string  albumNameDefault = "";
    [ObservableProperty] private string  albumNameNew = "";
    [ObservableProperty] private AlbumLocal albumForSync;
    [ObservableProperty] private ObservableCollection<AlbumRemote> albumsRemoteOwn = [];

    private readonly Popup _popup;

    public AlbumSyncPopupViewModel(Popup popup, AlbumLocal album)
    {
        _popup = popup;
        AlbumForSync = album;
        AlbumNameDefault = album.Name;
        AlbumNameNew = AlbumNameDefault;
        RequestRemoteAlbumsCommand.Execute(null);
    }

    public bool CanRequestRemote => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRequestRemote))]
    public async Task RequestRemoteAlbums()
    {
        (var res, var body) = await FetchAsync<AlbumsResponse>(
            HttpMethod.Get, URLs.Albums,
            f => IsBusy = f, e => Error = e
        );

        if (body == null) return;

        AlbumsRemoteOwn = new(body.Own);
        bool canSetNewName = AlbumNameDefault == AlbumNameNew;
        AlbumNameDefault = GetUniqueName(body.Own.Select(a => a.Name).ToList(), AlbumForSync.Name);
        if (canSetNewName)
            AlbumNameNew = AlbumNameDefault;
        
        AlbumsListIsVisible = AlbumsRemoteOwn.Count > 0;
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }

    [RelayCommand(IncludeCancelCommand = true)]
    public async Task Confirm(AlbumRemote? remoteAlbum = null, CancellationToken token = default)
    {
        if (remoteAlbum == null)
        {
            string finalName;
            if (AlbumNameNew != "")
            {
                string enteredName = AlbumNameNew;
                AlbumNameNew = AlbumNameDefault = GetUniqueName(AlbumsRemoteOwn.Select(a => a.Name).ToList(), enteredName);
                if (enteredName != AlbumNameNew)
                    return;

                finalName = enteredName;
            }
            else
            {
                finalName = AlbumNameDefault;
            }

            (var res, var body) = await FetchAsync<AlbumResponse>(
                HttpMethod.Post, URLs.Albums,
                f => IsBusy = f, e => Error = e,
                new AlbumCreateRequest(finalName), 
                serialize: true,
                cancellationToken: token
            );

            if (body == null) return;

            remoteAlbum = body.Album;
        }

        AlbumSynced syncedAlbum = new(AlbumForSync, remoteAlbum);
        RemoteAlbumsData.AlbumsOwn.Remove(remoteAlbum);
        LocalData.Albums.Remove(AlbumForSync);
        LocalData.Albums.Add(syncedAlbum);
        DB.Insert(syncedAlbum);

        _popup.Close(syncedAlbum);
    }

    public static string GetUniqueName(List<string> existingNames, string newName)
    {
        Dictionary<string, int> nameCounts = [];

        foreach (var name in existingNames)
        {
            string baseName = name;
            int count = 1;

            int lastOpenParen  = name.LastIndexOf('(');
            int lastCloseParen = name.LastIndexOf(')');

            if (lastOpenParen > 0 && lastCloseParen > lastOpenParen)
            {
                string countStr = name.Substring(lastOpenParen + 1, lastCloseParen - lastOpenParen - 1);
                if (int.TryParse(countStr, out int parsedCount))
                {
                    baseName = name.Substring(0, lastOpenParen).Trim();
                    count = parsedCount;
                }
            }

            if (nameCounts.ContainsKey(baseName))
            {
                if (count > nameCounts[baseName])
                {
                    nameCounts[baseName] = count;
                }
            }
            else
            {
                nameCounts[baseName] = count;
            }
        }

        if (nameCounts.ContainsKey(newName))
        {
            int nextCount = nameCounts[newName] + 1;
            nameCounts[newName] = nextCount;
            return $"{newName} ({nextCount})";
        }
        else
        {
            nameCounts[newName] = 1;
            return newName;
        }
    }
}

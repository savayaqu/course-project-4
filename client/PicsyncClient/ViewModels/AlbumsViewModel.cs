using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models;
using PicsyncClient.Models.Response;
using PicsyncClient.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PicsyncClient.ViewModels;

public partial class AlbumsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Album> albumsFiltered   = [];
    [ObservableProperty] private List<Album>                 albumsOwn        = [];
    [ObservableProperty] private List<Album>                 albumsAccesseble = [];
    [ObservableProperty] private bool                        isFetch          = false;
    [ObservableProperty] private string?                     error            = null;

    public AlbumsViewModel()
    {
        RequestAlbums();
    }

    public async void RequestAlbums()
    {
        (var res, var body) = await Fetch.DoAsync<AlbumsResponse>(
            HttpMethod.Get, "albums",
            isFetch => IsFetch = isFetch,
            error   => Error   = error
        );
        if (body == null) return;
        AlbumsOwn        = body.Own;
        AlbumsAccesseble = body.Accessible;
        foreach (var album in AlbumsOwn       ) album.FillPreview();
        foreach (var album in AlbumsAccesseble) album.FillPreview();

        AlbumsFiltered = new(AlbumsOwn.Concat(AlbumsAccesseble));
        Debug.WriteLine(AlbumsFiltered[0].Preview.Pictures[0].Thumbnail.ToString());
    }
}

using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Response;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.Utils;

public static class RemoteAlbumsData
{
    public static DateTime? GotAt { get; set; } = null;
    public static ObservableCollection<AlbumRemote> AlbumsOwn        { get; } = [];
    public static ObservableCollection<AlbumRemote> AlbumsAccessible { get; } = [];

    public static async Task FillAlbums(
        Action<bool>? setIsFetch = null, 
        Action<string>? setError = null, 
        CancellationToken cancellationToken = default
    ) {
        (var res, var body) = await FetchAsync<AlbumsResponse>(
            HttpMethod.Get, URLs.Albums,
            setIsFetch, setError,
            cancellationToken: cancellationToken
        );

        if (body == null)
        {
            Debug.WriteLine($"RemoteAlbumsData: FillAlbums: body empty\n{JsonSerializer.Serialize(res)}");
            return;
        }

        GotAt = DateTime.Now;

        AlbumsOwn.Clear(); // TODO: Clear() нверное плохо
        AlbumsAccessible.Clear();

        foreach (var album in body.Accessible)
            AlbumsAccessible.Add(album);

        foreach (var album in body.Own)
            AlbumsOwn.Add(album);
    }
}
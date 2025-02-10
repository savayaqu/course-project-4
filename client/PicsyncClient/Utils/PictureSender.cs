using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Response;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using static PicsyncClient.Utils.Fetcher;
using static PicsyncClient.Utils.LocalDB;
using PicsyncClient.Models.Pictures;
using System.Collections.Concurrent;
using PicsyncClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Collections.Specialized;
using System;

namespace PicsyncClient.Utils;

public partial class PictureSender : ObservableObject
{
    public static readonly PictureSender Default = new();

    public ObservableCollection<UploadsAlbum> UploadsAlbums { get; } = [];

    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private bool isUploading = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    public int uploadedCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RemainingBytes))]
    [NotifyPropertyChangedFor(nameof(AverageSpeed))]
    [NotifyPropertyChangedFor(nameof(ETA))]
    public ulong uploadedBytes = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AverageSpeed))]
    [NotifyPropertyChangedFor(nameof(ETA))]
    public TimeSpan timeSpent = TimeSpan.Zero;

    public double Progress => (double)UploadedCount / TotalCount;

    public int TotalCount => UploadsAlbums.Aggregate(0, (sum, uplAlb) => sum + uplAlb.Uploads.Count);

    public ulong TotalBytes => UploadsAlbums
        .SelectMany(a => a.Uploads)
        .Aggregate(0UL, (sum, u) => sum + u.Item.Size);

    public ulong RemainingBytes => TotalBytes - UploadedBytes;

    public ulong AverageSpeed => (ulong)(UploadedBytes / TimeSpent.TotalSeconds);

    public TimeSpan ETA => AverageSpeed == 0 
        ? TimeSpan.Zero 
        : TimeSpan.FromSeconds((double)RemainingBytes / AverageSpeed);

    private void OnUploadsChanged()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(TotalBytes));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(RemainingBytes));
        OnPropertyChanged(nameof(ETA));
    }

    public void AllManualAlbumUpload()
    {
        var albums = LocalData.Albums.OfType<AlbumSynced>().ToList();

        foreach (var album in albums)
        {
            if (album.LocalPictures.OfType<PictureLocal>().Any())
                ManualAlbumUpload(album);
        }
    }

    public UploadsAlbum ManualAlbumUpload(AlbumSynced album)
    {
        var uploadsAlbum = UploadsAlbums.Where(uplAlb => uplAlb.Album == album).FirstOrDefault();

        if (uploadsAlbum == null)
        {
            uploadsAlbum = new(album, true);
            UploadsAlbums.Add(uploadsAlbum);
            OnUploadsChanged();
            return uploadsAlbum;
        }

        var pictures = album.LocalPictures.OfType<PictureLocal>().ToList();
        foreach (var picture in pictures)
        {
            if (uploadsAlbum.UploadQueue.Where(uplPic => uplPic.Item == picture).Any())
                continue;

            UploadItem<PictureLocal> uploadItem = new(picture);
            uploadsAlbum.Uploads.Add(uploadItem);
            uploadsAlbum.UploadQueue.Enqueue(uploadItem);
        }
        OnUploadsChanged();
        return uploadsAlbum;
    }

    public void StartUploadIfNotActive()
    {
        Debug.WriteLine("StartUploadIfNotActive: IsUploading: " + IsUploading);
        if (IsUploading) return;

        _ = ProcessUploadQueueAsync();
    }

    private async Task ProcessUploadQueueAsync()
    {
        IsUploading = true;
        _cts = new CancellationTokenSource();

        Debug.WriteLine("ProcessUploadQueueAsync: START");

        foreach (var uploadsAlbum in UploadsAlbums)
        {
            if (_cts.Token.IsCancellationRequested)
                break;

            while (uploadsAlbum.UploadQueue.TryDequeue(out var uploadItem))
            {

                Debug.WriteLine("ProcessUploadQueueAsync: TryDequeue: " + JsonSerializer.Serialize(uploadItem));

                await UploadPictureAsync(uploadItem, uploadsAlbum);

                Debug.WriteLine("ProcessUploadQueueAsync: TryDequeue: END");

                if (_cts.Token.IsCancellationRequested)
                    break;
            }
        }

        Debug.WriteLine("ProcessUploadQueueAsync: END");

        IsUploading = false;

        _cts.Dispose();
        _cts = null;
    }

    public void StopUpload()
    {
        _cts?.Cancel();
        IsUploading = false;
    }

    public void ClearQueue()
    {
        StopUpload();
        UploadedCount = 0;
        UploadedBytes = 0;
        UploadsAlbums.Clear();
        OnUploadsChanged();
    }

    public async Task<bool?> UploadPictureAsync(
        UploadItem<PictureLocal> uploadedPicture, 
        UploadsAlbum uploadsAlbum
    ) {
        var localPicture = uploadedPicture.Item;
        AlbumSynced syncedAlbum = uploadsAlbum.Album;

        ProgressStreamContent streamContent = new(File.OpenRead(localPicture.LocalPath));
        streamContent.ProgressChanged += (bytes, currBytes, totalBytes) => 
            uploadedPicture.Progress = (double)currBytes / totalBytes; //* 100;

        MultipartFormDataContent content = new()
        {
            {
                streamContent, 
                "pictures[0][file]", 
                localPicture.Name 
            },
            {
                new StringContent(localPicture.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
                "pictures[0][date]"
            },
            {
                new StringContent(localPicture.Name),
                "pictures[0][name]"
            }
        };

        string? error = null;

        var startedAt = DateTime.Now;
        uploadedPicture.StartedAt = DateTime.Now;

        (var res, var body) = await FetchAsync<PicturesSendResponse>(
            HttpMethod.Post,
            URLs.AlbumPictures(syncedAlbum.Id),
            setError: e => error = e,
            body: content,
            cancellationToken: _cts?.Token ?? CancellationToken.None
        );

        var currentTimeSpent = DateTime.Now - startedAt; // TODO: ожидание сервера игнорировать

        TimeSpent += currentTimeSpent;
        UploadedCount++;
        UploadedBytes += uploadedPicture.Item.Size;
        uploadsAlbum.UploadedCount++;
        uploadedPicture.Progress = 1;
        uploadedPicture.TimeSpent = currentTimeSpent;

        if (body?.Successful?.Count > 0)
        {
            var remotePicture = body.Successful[0];

            PictureSynced syncedPicture = new(localPicture, remotePicture, syncedAlbum);

            LocalData.       Pictures.ReplaceOrAdd(localPicture, syncedPicture);
            syncedAlbum.LocalPictures.ReplaceOrAdd(localPicture, syncedPicture);

            DB.Insert(syncedPicture);

            return true;
        }
        else if (body?.Errored?.Count > 0)
        {
            var errorPart = body.Errored[0];

            uploadedPicture.Error = errorPart.Message;

            if (errorPart.Picture == null) return false;

            // TODO: а если две картинки с одним и тем же хешем?


            var syncedAlready = DB
                .Table<PictureSynced>()
                .Where(p => 
                    (p.Id == errorPart.Picture.Id) ||
                    (p.Hash == errorPart.Picture.Hash)
                )
                .FirstOrDefault();

            if (syncedAlready != null)
            {
                bool isDuplica = syncedAlbum.LocalPictures
                    .OfType<PictureSynced>()
                    .Where(p => p.Hash == errorPart.Picture.Hash)
                    .Any();

                if (isDuplica)
                {
                    DB.Insert(new PictureDuplica(localPicture.LocalPath));
                    LocalData.       Pictures.Remove(localPicture);
                    syncedAlbum.LocalPictures.Remove(localPicture);
                    return false;
                }

                syncedAlready.Album = syncedAlbum;
                syncedAlready.Update(localPicture);
                syncedAlready.Update(errorPart.Picture);
                
                LocalData.       Pictures.ReplaceOrAdd(localPicture, syncedAlready);
                syncedAlbum.LocalPictures.ReplaceOrAdd(localPicture, syncedAlready);

                DB.Update(syncedAlready);

                return true;
            }

            PictureSynced syncedPicture = new(localPicture, errorPart.Picture, syncedAlbum);

            LocalData.       Pictures.ReplaceOrAdd(localPicture, syncedPicture);
            syncedAlbum.LocalPictures.ReplaceOrAdd(localPicture, syncedPicture);

            DB.Insert(syncedPicture);

            return true;
        }
        else
        {
            uploadedPicture.Error = error ?? "Произошла неизвестная ошибка";
        }
        return false;
    }
}
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
using System.IO;
using System;

namespace PicsyncClient.Utils;

public static class PictureSender
{
    private readonly static ConcurrentQueue<UploadItem<PictureLocal>> _uploadQueue = [];
    private static bool _isUploading = false;

    public readonly static ObservableCollection<UploadItem<PictureLocal>> Uploads = [];

    public static void AddPicturesToQueue(IEnumerable<PictureLocal> pictures, ulong? albumId = null)
    {
        foreach (var picture in pictures)
        {
            Debug.WriteLine("AddPicturesToQueue: picture: " + JsonSerializer.Serialize(picture));
            if (albumId == null || picture.Album is not AlbumSynced)
            {
                Debug.WriteLine("Необходимо задать удалённый альбом для отправки!");
                continue;
            }

            UploadItem<PictureLocal> uploadItem = new(picture);

            _uploadQueue.Enqueue(uploadItem);
            Uploads.Add(uploadItem);
        }

        StartUploadIfNotActive();
    }

    private static void StartUploadIfNotActive()
    {
        Debug.WriteLine("StartUploadIfNotActive: " + _isUploading);
        if (_isUploading) return;

        _ = ProcessUploadQueueAsync();
    }

    private static async Task ProcessUploadQueueAsync()
    {
        _isUploading = true;

        Debug.WriteLine("ProcessUploadQueueAsync: START");

        while (_uploadQueue.TryDequeue(out var uploadItem))
        {
            Debug.WriteLine("ProcessUploadQueueAsync: TryDequeue: " + JsonSerializer.Serialize(uploadItem));
            await UploadPictureAsync(uploadItem);
            Debug.WriteLine("ProcessUploadQueueAsync: TryDequeue: END");
        }

        Debug.WriteLine("ProcessUploadQueueAsync: END");

        _isUploading = false;
    }

    public static async Task<bool?> UploadPictureAsync(
        UploadItem<PictureLocal> uploadedPicture, 
        ulong? albumId = null, 
        CancellationToken token = default
    ) {
        var localPicture = uploadedPicture.Item;
        AlbumSynced? syncedAlbum = null;

        if (albumId == null)
        {
            if (localPicture.Album is not AlbumSynced album)
            {
                Debug.WriteLine("Необходимо задать удалённый альбом для отправки!");
                return default;
            }
            syncedAlbum = album;
            albumId = album.Id;
        }

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
        };

        if (localPicture.Date is DateTime date)
            content.Add(
                new StringContent(date.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")), 
                "pictures[0][date]"
            );

        string? error = null;

        (var res, var body) = await FetchAsync<PicturesSendResponse>(
            HttpMethod.Post, 
            URLs.AlbumPictures((ulong)albumId),
            setError: e => error = e,
            body: content,
            cancellationToken: token
        );
        
        if (body?.Successful?.Count > 0)
        {
            var remotePicture = body.Successful[0];

            uploadedPicture.Progress = 1;
            if (syncedAlbum != null)
            {
                PictureSynced syncedPicture = new(localPicture, remotePicture, syncedAlbum);
                LocalData.Pictures.Remove(localPicture);

                syncedAlbum.LocalPictures.Remove(syncedPicture);
                syncedAlbum.LocalPictures.Add(syncedPicture);
                DB.Insert(syncedPicture);
            }
            return true;
        }
        else if (body?.Errored?.Count > 0)
        {
            var errorPart = body.Errored[0];

            uploadedPicture.Error = errorPart.Message;
            if (syncedAlbum != null && errorPart.Picture != null)
            {
                // TODO: а если две картинки с одним и тем же хешем?
                PictureSynced syncedPicture = new(localPicture, errorPart.Picture, syncedAlbum);
                LocalData.Pictures.Remove(localPicture);

                syncedAlbum.LocalPictures.Remove(syncedPicture);
                syncedAlbum.LocalPictures.Add(syncedPicture);
                DB.Insert(syncedPicture);
                return true;
            }
        }
        else
        {
            uploadedPicture.Error = error ?? "Произошла неизвестная ошибка";
        }
        return false;
    }
}
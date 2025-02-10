using CommunityToolkit.Mvvm.ComponentModel;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace PicsyncClient.Models;

public partial class UploadsAlbum : ObservableObject
{
    public readonly ConcurrentQueue<UploadItem<PictureLocal>> UploadQueue = [];
    public required AlbumSynced Album { get; set; }
    public ObservableCollection<UploadItem<PictureLocal>> Uploads { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Progress))]
    [NotifyPropertyChangedFor(nameof(ETA))]
    public int uploadedCount = 0;

    [ObservableProperty]
    public ulong averageSpeed = 0;

    [SetsRequiredMembers]
    public UploadsAlbum(AlbumSynced album, IEnumerable<UploadItem<PictureLocal>>? uploadPictures = null)
    {
        Album = album;

        if (uploadPictures != null)
            Uploads = new(uploadPictures);

        Uploads.CollectionChanged += OnUploadsChanged;
    }

    [SetsRequiredMembers]
    public UploadsAlbum(AlbumSynced album, bool uploadFromLocal)
    {
        Album = album;

        if (uploadFromLocal)
        {
            var pictures = album.LocalPictures.OfType<PictureLocal>().ToList();
            foreach (var picture in pictures)
            {
                UploadItem<PictureLocal> uploadItem = new(picture);
                Uploads.Add(uploadItem);
                UploadQueue.Enqueue(uploadItem);
            }
        }

        Uploads.CollectionChanged += OnUploadsChanged;
    }

    private void OnUploadsChanged (object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(TotalBytes));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ETA));
    }

    public int TotalCount => Uploads.Count;

    public ulong TotalBytes => Uploads.Aggregate(0UL, (sum, u) => sum + u.Item.Size);

    public double Progress => (double)UploadedCount / TotalCount;

    public TimeSpan ETA
    {
        get
        {
            if (UploadedCount == 0)
                return TimeSpan.Zero;

            var completedUploads = Uploads.Where(u => u.TimeSpent != default).ToList();
            if (completedUploads.Count == 0)
                return TimeSpan.Zero;

            ulong totalBytesUploaded = completedUploads.Aggregate(0UL, (sum, u) => sum + u.Item.Size);

            TimeSpan totalTimeSpent = completedUploads
                .Where(u => u.TimeSpent != default)
                .Aggregate(TimeSpan.Zero, (sum, u) =>
                {
                    if (u.TimeSpent is not TimeSpan spent) 
                        return sum;

                    return sum + spent;
                });

            if (totalTimeSpent == TimeSpan.Zero)
                return TimeSpan.Zero;

            AverageSpeed = (ulong)(totalBytesUploaded / totalTimeSpent.TotalSeconds); // bytes per second

            ulong remainingBytes = Uploads
                .Where(u => u.TimeSpent == default)
                .Aggregate(0UL, (sum, u) => sum + u.Item.Size);

            return TimeSpan.FromSeconds(remainingBytes / AverageSpeed);
        }
    }
}
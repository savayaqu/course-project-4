using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PicsyncClient.Models;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using PicsyncClient.Models.Response;
using PicsyncClient.Views;
using PicsyncClient.Utils;
using PicsyncClient.Enum;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.ViewModels;

public partial class AlbumViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    private bool isBusy = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    private int localOffset = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    private int remotePage = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    private PicturesPageResponse? latestRemotePicturesPage;

    //public int PageSize => ColumnCount * 10;
    public int PageSize => 30;

    [ObservableProperty]
    private IAlbum album;

    [ObservableProperty]
    private string? error;

    [ObservableProperty]
    private ObservableCollection<ItemssGroup<Models.Pictures.IPicture>> picturesGroups = [];

    public AlbumViewModel(IAlbum album)
    {
        Album = album;
        if (CanLoadMore)
            LoadMore();
    }

    public bool CanLoadMore 
    { 
        get
        {
            if (IsBusy) return false;
            bool res = false;
            if (Album is AlbumLocal albumLocal)
            {
                res = albumLocal.LocalPictures.Count > LocalOffset;
                Debug.WriteLine($"CanLoadMoreCheck: {res} = albumLocal.LocalPictures.Count {albumLocal.LocalPictures.Count} > LocalOffset {LocalOffset}");
            }
            else if (Album is AlbumRemote albumRemote)
            {
                res = albumRemote.RemotePicturesCount > (RemotePage - 1) * PageSize;
                Debug.WriteLine($"CanLoadMoreCheck: {res} = albumRemote.RemotePicturesCount {albumRemote.RemotePicturesCount} > (RemotePage {RemotePage} - 1) * PageSize {PageSize}");
            }
            else
            {
                res = false;
                Debug.WriteLine($"CanLoadMoreCheck: {res} = NOT IMPLEMENTED");
            }
            return res;
        } 
    }

    [RelayCommand(CanExecute = nameof(CanLoadMore))]
    public async Task LoadMore()
    {
        Debug.WriteLine("=========== LoadMore ============");
        Debug.WriteLine($"Offset & Page: {LocalOffset} & {RemotePage}, IsBusy: {IsBusy}");
        if (IsBusy) return;

        IsBusy = true;
        List<Models.Pictures.IPicture> piece;
        
        if (Album is AlbumLocal albumLocal)
        {
            piece = albumLocal.LocalPictures
                .Skip(LocalOffset)
                .Take(PageSize)
                .Cast<Models.Pictures.IPicture>()
                .ToList();

            LocalOffset += PageSize;
        }
        else if (Album is AlbumRemote albumRemote)
        {
            (var res, var picturesPage) = await FetchAsync<PicturesPageResponse>(
                HttpMethod.Get,
                URLs.AlbumPictures(albumRemote.Id, new()
                {
                    Page = RemotePage,
                    Limit = PageSize,
                    Sort = PicturesSort.date,
                    IsReverse = false,
                }),
                isFetch => IsBusy = isFetch,
                error => Error = error
            );
            if (picturesPage == null) return;

            if (picturesPage.Signature != null)
            {
                if (albumRemote.Preview == null)
                    albumRemote.Preview = new() { Signature = picturesPage.Signature };
                else
                    albumRemote.Preview.Signature = picturesPage.Signature;
            }

            piece = new(picturesPage.Pictures.Count);
            foreach (var picture in picturesPage.Pictures)
            {
                picture.Album = Album;
                piece.Add(picture);
            }

            RemotePage++;
        }
        else
        {
            throw new NotImplementedException();
        }

        //Debug.WriteLine($"FirstInPage: {piece?[0]?.Name}");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var picture in piece)
            {
                string? currGroupTitle = picture.Date?.ToString("MMMM, yyyy");
                string? lastGroupTitle = null;

                int lastGroupIndex = PicturesGroups.Count - 1;
                if (lastGroupIndex >= 0)
                    lastGroupTitle = PicturesGroups?[lastGroupIndex].Title;

                if (lastGroupTitle == currGroupTitle)
                    PicturesGroups[lastGroupIndex].Add(picture);
                else
                    PicturesGroups.Add(new(currGroupTitle ?? "", [picture]));

                Debug.WriteLine($"Added: {picture.Name}, to: {currGroupTitle}");
            }
        });

        IsBusy = false;
        Debug.WriteLine("=========== END LoadMore ============");
    }


    [ObservableProperty]
    private int columnCount = 3;

    [ObservableProperty]
    private double? requestColumnWidth = 120;

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

    [RelayCommand]
    private async void OpenViewer(Models.Pictures.IPicture picture)
    {
        await Shell.Current.Navigation.PushAsync(new ViewerPage(picture), false);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PicsyncClient.Models;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using PicsyncClient.Models.Response;
using PicsyncClient.Components.Popups;
using PicsyncClient.Views;
using PicsyncClient.Utils;
using PicsyncClient.Enum;
using static PicsyncClient.Utils.Fetcher;
using static PicsyncClient.Utils.LocalDB;
using System.Text.Json;

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
    private int remoteOffset = 0;

    [ObservableProperty]
    private PicturesPageResponse? lastRemotePicturesPage;

    //public int PageSize => ColumnCount * 10;
    public int PageSize => 30;

    [ObservableProperty]
    private IAlbum album;

    [ObservableProperty]
    private string? error;

    [ObservableProperty]
    private ObservableCollection<ItemsGroup<Models.Pictures.IPicture>> picturesGroups = [];

    public AlbumViewModel(IAlbum album)
    {
        Album = album;
        if (CanLoadMore)
            LoadMoreCommand.Execute(null);
    }


    public bool CanLoadMore 
    { 
        get
        {
            if (IsBusy) return false;
            if (Album is AlbumSynced albumSynced)
            {
                Debug.WriteLine($"CanLoadMoreCheck: {LastRemotePicturesPage == null ||
                    albumSynced.RemotePicturesCount > RemoteOffset ||
                    albumSynced.LocalPictures.Count > LocalOffset } = \n" +
                    $"LastRemotePicturesPage == null {LastRemotePicturesPage == null} ||\n" +
                    $"albumSynced.RemotePicturesCount {albumSynced.RemotePicturesCount} > RemoteOffset {RemoteOffset} ||\n" +
                    $"albumLocal .LocalPictures.Count {albumSynced.LocalPictures.Count} > LocalOffset {  LocalOffset}");
                if (LastRemotePicturesPage == null ||
                    albumSynced.RemotePicturesCount > RemoteOffset || 
                    albumSynced.LocalPictures.Count > LocalOffset) return true;
            }
            else if (Album is AlbumLocal albumLocal)
            {
                Debug.WriteLine($"CanLoadMoreCheck: {albumLocal.LocalPictures.Count > LocalOffset} = " +
                    $"albumLocal.LocalPictures.Count {albumLocal.LocalPictures.Count} > LocalOffset {LocalOffset}");
                return albumLocal.LocalPictures.Count > LocalOffset;
            }
            else if (Album is AlbumRemote albumRemote)
            {
                Debug.WriteLine($"CanLoadMoreCheck: {albumRemote.RemotePicturesCount > RemoteOffset} = " +
                    $"albumRemote.RemotePicturesCount {albumRemote.RemotePicturesCount} > RemoteOffset {RemoteOffset}");
                return albumRemote.RemotePicturesCount > RemoteOffset;
            }
            return false;
        } 
    }

    [RelayCommand(CanExecute = nameof(CanLoadMore))]
    public async Task LoadMore()
    {
        try
        {
            Debug.WriteLine($"=========== LoadMore {IsBusy} ============\nOffset / Total:\n" +
                ((Album is IAlbumLocal local) ? $"[local_] {LocalOffset}/{local.LocalPictures.Count}\n" : "") +
                ((Album is AlbumRemote remote) ? $"[remote] {RemoteOffset}/{remote.RemotePicturesCount}\n" : ""));

            if (!CanLoadMore) return;
            IsBusy = true;

            List<Models.Pictures.IPicture> piece;

            if (Album is AlbumSynced albumSynced)
            {
                // TODO: хочется отображать сразу локальные картинки, не дожидаясь 

                piece = new(PageSize);
                while (piece.Count < PageSize)
                {
                    // Элементы выбора
                    IPictureLocal? localPicture = null;
                    if (LocalOffset < albumSynced.LocalPictures.Count)
                        localPicture = albumSynced.LocalPictures[LocalOffset];

                    PictureRemote? remotePicture = null;
                    if (LastRemotePicturesPage == null || RemoteOffset < albumSynced.RemotePicturesCount)
                    {
                        int requiredPage = RemoteOffset / PageSize + 1;

                        // Запрашиваем удалённые если надо
                        if (LastRemotePicturesPage == null || LastRemotePicturesPage.Page != requiredPage)
                        {
                            (var res, var picturesPage) = await FetchAsync<PicturesPageResponse>(
                                HttpMethod.Get,
                                URLs.AlbumPictures(albumSynced.Id, new()
                                {
                                    Page = requiredPage,
                                    Limit = PageSize,
                                    Sort = PicturesSort.date,
                                    IsReverse = true,
                                }),
                                setError: e => Error = e
                            );
                            if (picturesPage != null)
                            {
                                albumSynced.RemotePicturesCount = picturesPage.Total;

                                if (picturesPage.Signature != null)
                                {
                                    if (albumSynced.Preview == null)
                                        albumSynced.Preview = new() { Signature = picturesPage.Signature };
                                    else
                                        albumSynced.Preview.Signature = picturesPage.Signature;
                                }

                                foreach (var picture in picturesPage.Pictures)
                                    picture.Album = Album;

                                LastRemotePicturesPage = picturesPage;
                            }
                        }
                        if (RemoteOffset < albumSynced.RemotePicturesCount)
                            remotePicture = LastRemotePicturesPage?.Pictures[RemoteOffset - PageSize * (RemoteOffset / PageSize)];
                    }

                    // Проверяем, существует ли локальная копия удалённой картинки
                    if (remotePicture != null)
                    {
                        var syncedPicture = albumSynced.LocalPictures
                            .OfType<PictureSynced>()
                            .FirstOrDefault(l => l.Id == remotePicture.Id);

                        if (syncedPicture != null)
                        {
                            syncedPicture.Update(remotePicture);
                            DB.Update(syncedPicture);
                            remotePicture = null;
                        }
                    }

                    // Выбираем, какой элемент добавить
                    if (remotePicture != null)
                    {
                        if (localPicture != null && localPicture.Date > remotePicture.Date)
                        {
                            LocalOffset++;
                            piece.Add(localPicture);
                        }
                        else
                        {
                            RemoteOffset++;
                            piece.Add(remotePicture);
                        }
                    }
                    else
                    {
                        if (localPicture != null)
                        {
                            LocalOffset++;
                            piece.Add(localPicture);
                        }
                        else
                        {
                            // Оба списка исчерпаны
                            break;
                        }
                    }
                }
                // TODO: переформировать более не синхронизируемые картинки
            }
            else if (Album is AlbumLocal albumLocal)
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
                        Page = RemoteOffset / PageSize + 1,
                        Limit = PageSize,
                        Sort = PicturesSort.date,
                        IsReverse = true,
                    }),
                    setError: e => Error = e
                );
                if (picturesPage == null) return;

                albumRemote.RemotePicturesCount = picturesPage.Total;

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

                RemoteOffset += PageSize;
            }
            else
            {
                throw new NotImplementedException();
            }

            //Debug.WriteLine($"FirstInPage: {piece?[0]?.Name}");

            //MainThread.BeginInvokeOnMainThread(() =>
            //{
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
            //});
            Debug.WriteLine($"LoadMore: piece: {JsonSerializer.Serialize(piece)}");

            //PicturesGroups.Add(new("test", [piece[5]]));

            IsBusy = false;
            Debug.WriteLine("=========== END LoadMore ============");
        }
        catch (Exception ex)
        {
            Shell.Current.DisplayAlert("DEBUG", ex.Message, "OK");
            Debug.WriteLine($"LoadMore: Ex:\n{ex.Message}");
        }
    }


    public bool IsLocal => Album is IAlbumLocal;
    public bool IsSynced => Album is AlbumSynced;
    public bool IsRemote => Album is AlbumRemote;
    public bool IsNonOwned => Album is AlbumRemote album && album.Owner != null;

    public bool CanSync => Album.GetType() == typeof(AlbumLocal);

    [RelayCommand(CanExecute = nameof(CanSync))]
    public async Task Sync()
    {
        if (Album is not AlbumLocal albumLocal || 
            albumLocal.GetType() != typeof(AlbumLocal)
        ) return;

        AlbumSyncPopup popup = new(albumLocal);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);
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

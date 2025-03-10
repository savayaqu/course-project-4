using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
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

namespace PicsyncClient.ViewModels;

public partial class AlbumViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    private int _localOffset;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoadMore))]
    private int _remoteOffset;

    [ObservableProperty]
    private PicturesPageResponse? _lastRemotePicturesPage;

    //public int PageSize => ColumnCount * 10; // Если бы были Offset'ные картинки в API
    public int PageSize => 30;

    [ObservableProperty] private string? _errorOnPage;
    [ObservableProperty] private string? _errorOnAlbum;

    [ObservableProperty]
    private ObservableCollection<ItemsGroup<Models.Pictures.IPicture>> _picturesGroups = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocal))]
    [NotifyPropertyChangedFor(nameof(IsSynced))]
    [NotifyPropertyChangedFor(nameof(IsRemote))]
    [NotifyPropertyChangedFor(nameof(IsNonOwned))]
    [NotifyPropertyChangedFor(nameof(IsRemoteOwned))]
    [NotifyPropertyChangedFor(nameof(CanSync))]
    private IAlbum _album;

    public bool IsLocal       => Album is IAlbumLocal;
    public bool IsSynced      => Album is AlbumSynced;
    public bool IsRemote      => Album is AlbumRemote;
    public bool IsNonOwned    => Album is AlbumRemote { Owner: not null };
    public bool IsRemoteOwned => Album is AlbumRemote { Owner: null };


    public AlbumViewModel(IAlbum album)
    {
        Album = album;
        //RefreshCommand.Execute(null);
        LoadInfoCommand.Execute(null);

        if (CanLoadMore)
            LoadMoreCommand.Execute(null);
    }


    [RelayCommand]
    private async Task Refresh()
    {
        OnPropertyChanged(nameof(Album));
        PicturesGroups.Clear(); // TODO: Очищать только в случае успеха запроса страницы
        LocalOffset = 0;
        RemoteOffset = 0;
        LastRemotePicturesPage = null;

        Task loadInfoTask = LoadInfoCommand.ExecuteAsync(null);

        if (CanLoadMore) 
            await LoadMoreCommand.ExecuteAsync(null);

        await loadInfoTask;
    }


    [RelayCommand]
    private async Task LoadInfo()
    {
        if (Album is not AlbumRemote remote) return;

        var (res, body) = await FetchAsync<AlbumResponse>(
            HttpMethod.Get, URLs.AlbumInfo(remote.Id),
            setError: e => ErrorOnAlbum = e
        );

        if (body == null) return;

        body.Album.GrantAccesses ??= [];
        body.Album.Invitations   ??= [];

        remote.Update(body.Album);
        OnPropertyChanged(nameof(Album));
    }


    public bool CanLoadMore 
    { 
        get
        {
            if (IsBusy) return false;
            if (Album is AlbumSynced albumSynced)
            {
                //Debug.WriteLine($"CanLoadMoreCheck: {LastRemotePicturesPage == null ||
                //    albumSynced.RemotePicturesCount > RemoteOffset ||
                //    albumSynced.LocalPictures.Count > LocalOffset } = " +
                //    $"LastRemotePicturesPage == null {LastRemotePicturesPage == null} || " +
                //    $"albumSynced.RemotePicturesCount {albumSynced.RemotePicturesCount} > RemoteOffset {RemoteOffset} || " +
                //    $"albumLocal .LocalPictures.Count {albumSynced.LocalPictures.Count} > LocalOffset {  LocalOffset}");
                if (LastRemotePicturesPage == null ||
                    albumSynced.RemotePicturesCount > RemoteOffset || 
                    albumSynced.LocalPictures.Count > LocalOffset) return true;
            }
            else if (Album is AlbumLocal albumLocal)
            {
                //Debug.WriteLine($"CanLoadMoreCheck: {albumLocal.LocalPictures.Count > LocalOffset} = " +
                //    $"albumLocal.LocalPictures.Count {albumLocal.LocalPictures.Count} > LocalOffset {LocalOffset}");
                return albumLocal.LocalPictures.Count > LocalOffset;
            }
            else if (Album is AlbumRemote albumRemote)
            {
                //Debug.WriteLine($"CanLoadMoreCheck: {albumRemote.RemotePicturesCount > RemoteOffset} = " +
                //    $"albumRemote.RemotePicturesCount {albumRemote.RemotePicturesCount} > RemoteOffset {RemoteOffset}");
                return albumRemote.RemotePicturesCount > RemoteOffset;
            }
            return false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadMore))]
    private async Task LoadMore()
    {
        Debug.WriteLine($"=========== LoadMore (IsBusy: {IsBusy}) ============\nOffset / Total:\n" +
            ((Album is IAlbumLocal  local) ? $"[local_] {LocalOffset }/{local .LocalPictures.Count}\n" : "") +
            ((Album is AlbumRemote remote) ? $"[remote] {RemoteOffset}/{remote.RemotePicturesCount}\n" : ""));

        if (!CanLoadMore) return;
        IsBusy = true;

        try
        {
            List<Models.Pictures.IPicture> piece;

            if (Album is AlbumSynced albumSynced)
            {
                // TODO: хочется отображать сразу локальные картинки, не дожидаясь 

                Debug.WriteLine("local of album: " + JsonSerializer.Serialize(albumSynced.LocalPictures));
                Debug.WriteLine("\n");
                //Debug.WriteLine("synced only: " + JsonSerializer.Serialize(albumSynced.LocalPictures.OfType<PictureSynced>()));

                var syncedPictures = albumSynced.LocalPictures
                    .OfType<PictureSynced>();

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
                            var (res, picturesPage) = await FetchAsync<PicturesPageResponse>(
                                HttpMethod.Get,
                                URLs.AlbumPictures(albumSynced.Id, new()
                                {
                                    Page = requiredPage,
                                    Limit = PageSize,
                                    Sort = PicturesSort.date,
                                    IsReverse = true,
                                }),
                                setError: e => ErrorOnPage = e
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
                            Debug.WriteLine("return from server: " + JsonSerializer.Serialize(picturesPage));
                        }
                        if (RemoteOffset < albumSynced.RemotePicturesCount)
                            remotePicture = LastRemotePicturesPage?.Pictures[RemoteOffset - PageSize * (RemoteOffset / PageSize)];
                    }

                    // Проверяем, существует ли локальная копия удалённой картинки
                    if (remotePicture != null)
                    {
                        var syncedPicture = syncedPictures.FirstOrDefault(l => l.Id == remotePicture.Id);

                        if (syncedPicture != null)
                        {
                            Debug.WriteLine("found synced of remote picture: " + JsonSerializer.Serialize(syncedPicture));
                            syncedPicture.Update(remotePicture);
                            DB.Update(syncedPicture);
                            remotePicture = null;
                            RemoteOffset++;
                        }
                    }

                    // Выбираем, какой элемент добавить
                    if (remotePicture != null)
                    {
                        if (localPicture != null && localPicture.Date > remotePicture.Date)
                        {
                            LocalOffset++;
                            piece.Add(localPicture);
                            Debug.WriteLine($"Add: /{localPicture.Name}");
                        }
                        else
                        {
                            RemoteOffset++;
                            piece.Add(remotePicture);
                            Debug.WriteLine($"Add: #{remotePicture.Id}:{remotePicture.Name}");
                        }
                    }
                    else
                    {
                        if (localPicture != null)
                        {
                            LocalOffset++;
                            piece.Add(localPicture);
                            Debug.WriteLine($"Add: /{localPicture.Name}");
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
                var (res, picturesPage) = await FetchAsync<PicturesPageResponse>(
                    HttpMethod.Get,
                    URLs.AlbumPictures(albumRemote.Id, new()
                    {
                        Page = RemoteOffset / PageSize + 1,
                        Limit = PageSize,
                        Sort = PicturesSort.date,
                        IsReverse = true,
                    }),
                    setError: e => ErrorOnPage = e
                );
                if (picturesPage == null)
                {
                    IsBusy = false;
                    return;
                }

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

            Debug.WriteLine("piece: " + JsonSerializer.Serialize(piece));

            //Debug.WriteLine($"FirstInPage: {piece?[0]?.Name}");

            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            foreach (var picture in piece)
            {
                string currGroupTitle = picture.Date.ToString("MMMM, yyyy");
                string? lastGroupTitle = null;
        
                int lastGroupIndex = PicturesGroups.Count - 1;
                if (lastGroupIndex >= 0)
                    lastGroupTitle = PicturesGroups[lastGroupIndex].Title;
        
                if (lastGroupTitle == currGroupTitle)
                    PicturesGroups[lastGroupIndex].Add(picture);
                else
                    PicturesGroups.Add(new(currGroupTitle, [picture]));
        
                Debug.WriteLine($"Added: {picture.Name}, to: {currGroupTitle}");
            }
            //});
            Debug.WriteLine($"LoadMore: piece: {JsonSerializer.Serialize(piece)}");

            //PicturesGroups.Add(new("test", [piece[5]]));

            Debug.WriteLine("=========== END LoadMore ============");
        }
        catch (Exception ex)
        {
            _ = Shell.Current.DisplayAlert("DEBUG", ex.Message, "OK");
            Debug.WriteLine($"LoadMore: Ex:\n{ex.Message}");
        }
        IsBusy = false;
    }


    [RelayCommand]
    private async Task OpenInfo()
    {
        AlbumInfoPopup popup = new(Album);
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        OnPropertyChanged(nameof(Album));
    }

    public bool CanSync => Album.GetType() == typeof(AlbumLocal);

    [RelayCommand(CanExecute = nameof(CanSync))]
    private async Task Sync()
    {
        if (Album is not AlbumLocal albumLocal || 
            albumLocal.GetType() != typeof(AlbumLocal)
        ) return;

        AlbumSyncPopup popup = new(albumLocal);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not AlbumSynced album) return;

        Album = album;
        RefreshCommand.Execute(null);
    }

    public bool CanSyncManage => Album is AlbumSynced;

    [RelayCommand(CanExecute = nameof(CanSyncManage))]
    private async Task SyncManage()
    {
        if (Album is not AlbumSynced synced) return;

        AlbumSyncManagePopup popup = new(synced);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not AlbumLocal local) return;

        // TODO: навигировать назад и разделить на remote и local
        Album = local;
        RefreshCommand.Execute(null);
    }

    public bool CanAccessManage => Album is AlbumRemote;

    [RelayCommand(CanExecute = nameof(CanAccessManage))]
    private async Task AccessManage()
    {
        if (Album is not AlbumRemote remote) return;

        AlbumAccessManagePopup popup = new(remote);
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        OnPropertyChanged(nameof(Album));
    }

    [RelayCommand]
    private async Task OpenViewer(Models.Pictures.IPicture picture)
    {
        await Shell.Current.Navigation.PushAsync(new ViewerPage(picture, this), false);
        await Task.Delay(500);
    }

    public bool CanUnjoin => Album is AlbumRemote;

    [RelayCommand(CanExecute = nameof(CanUnjoin))]
    private async Task Unjoin()
    {
        if (Album is not AlbumRemote remote) return;

        AlbumUnjoinPopup popup = new(remote);
        var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);

        if (result is not (bool and true)) return;
        await Shell.Current.GoToAsync("..");
    }


    [ObservableProperty]
    private int _columnCount = 1;

    private double? _requestColumnWidth = 120;
    private double? RequestColumnWidth
    {
        get => _requestColumnWidth;
        set
        {
            SetProperty(ref _requestColumnWidth, value);
            CalculateColumnsWidthCommand.Execute(_containerWidth);
        }
    }

    [ObservableProperty]
    private double _columnWidth = 100;

    private double _containerWidth = 100;

    [RelayCommand]
    private void CalculateColumnsWidth(double containerWidth)
    {
        _containerWidth = containerWidth;
        if (RequestColumnWidth != null)
            ColumnCount = Math.Max((int)(containerWidth / RequestColumnWidth), 1);

        ColumnWidth = containerWidth / ColumnCount;
        Debug.WriteLine($"=== ChgColW ===\n{ColumnWidth} = {containerWidth} / {ColumnCount}");
    }
}

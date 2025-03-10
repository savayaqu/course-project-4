using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using PicsyncClient.Components.Popups;
using PicsyncClient.Views;
using PicsyncClient.Utils;
using PicsyncClient.Enum;
using CommunityToolkit.Mvvm.Collections;

namespace PicsyncClient.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly MainPage? _contentPage;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _hasSynced;

    [ObservableProperty]
    private ObservableGroupedCollection<DateOnly, IPictureLocal> _picturesGroups = [];

    private List<IPictureLocal> PicturesFlat { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalCount))]
    [NotifyPropertyChangedFor(nameof(SyncedCount))]
    private ObservableCollection<AlbumSynced> _albumsSynced = [];

    public MainPageViewModel(MainPage? contentPage = null)
    {
        _contentPage = contentPage;
        RefreshCommand.Execute(null);
    }

    public int AllCount    => AlbumsSynced.Sum(a => a.PicturesCount);

    public int LocalCount  => AlbumsSynced.Sum(a => a.TrueLocalPicturesCount);

    public int SyncedCount => AlbumsSynced.Sum(a => a.SyncedPicturesCount);

    public int PageSize => ColumnCount * 10;

    [ObservableProperty]
    private bool _isRefreshing;

    [RelayCommand]
    public async Task Refresh()
    {
        IsRefreshing = true;

        CanLoadMore = false;
        await RequestLocalAlbums();

        HasSynced = AlbumsSynced.Any();
        if (HasSynced)
        {
            PicturesCursors = null;
            CanLoadMore = true;
        }
        IsRefreshing = false;
    }


    [RelayCommand]
    private async Task SyncManage()
    {
        GeneralSyncManagePopup popup = new();
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
    }


    [ObservableProperty]
    private bool _hasPermissions;


    [RelayCommand]
    private async Task GoToAlbums()
    {
        try
        {
            HasPermissions = await LocalData.RequestPermissions();
            if (HasPermissions)
                _ = Shell.Current.GoToAsync("//Albums");
            else
                _ = Shell.Current.DisplayAlert("Ошибка", "Вы должны дать разрешение на просмотр локальных картинок", "OK");
        }
        catch (PlatformNotSupportedException ex)
        {
            _ = Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task RequestLocalAlbums()
    {
        // Если не разрешено или не поддерживается — произойдёт исключение
        try
        {
            HasPermissions = await LocalData.CheckPermissions();
            if (!HasPermissions)
                return;
        }
        catch (PlatformNotSupportedException)
        {
            return;
        }

        if (LocalData.Status == LocalLoadStatus.NotLoad)
        {
            await LocalData.FillPictures();
        }
        else if (LocalData.Status == LocalLoadStatus.InLoad)
        {
            // TODO: сделать подписку на изменение
            while (LocalData.Status == LocalLoadStatus.InLoad)
            {
                await Task.Delay(1000);
            }
        }
        else if (LocalData.Status == LocalLoadStatus.Loaded)
        {
            await LocalData.FillPictures();
        }
        AlbumsSynced = new(LocalData.Albums.OfType<AlbumSynced>().ToList());

        PicturesFlat = AlbumsSynced
            .SelectMany(album => album.LocalPictures)
            .OrderByDescending(picture => picture.Date)
            .ToList();

        PicturesGroups = new(PicturesFlat
            .GroupBy(picture => new DateOnly(
                picture.Date.Year, 
                picture.Date.Month, 
                1
            ))
            .ToList()
        );
    }

    [RelayCommand]
    private async Task OpenViewer(IPictureLocal picture)
    {
        await Shell.Current.Navigation.PushAsync(
            new ViewerMainPage(
                picture,
                PicturesFlat
                    .Cast<Models.Pictures.IPicture>()
                    .ToList(),
                pictureOut => _contentPage?.ScrollTo(pictureOut)
            ),
            false
        );
        await Task.Delay(500);
    }

    public void LightUpdate()
    {
        AlbumsSynced.UpdateFrom(LocalData.Albums.OfType<AlbumSynced>().ToList());

        PicturesFlat
        .UpdateFrom(AlbumsSynced
            .SelectMany(album => album.LocalPictures)
            .OrderByDescending(picture => picture.Date)
            .ToList()
        );

        PicturesGroups
        .SyncWith(new ObservableGroupedCollection<DateOnly, IPictureLocal>(
            PicturesFlat
            .GroupBy(picture => new DateOnly(
                picture.Date.Year,
                picture.Date.Month,
                1
            )
        )));
    }


    public List<IEnumerator<IPictureLocal>>? PicturesCursors;

    [ObservableProperty]
    //[NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    private bool _canLoadMore;

    /*
    [RelayCommand(CanExecute = nameof(CanLoadMore))]
    public async Task LoadMore()
    {
        Debug.WriteLine("=========== START LoadMore ============");
        
        try
        {
            if (!CanLoadMore) return;
            CanLoadMore = false;

            List<IPictureLocal> piece = new(PageSize);

            if (PicturesCursors == null)
            {
                PicturesCursors = new(AlbumsSynced.Count);

                foreach (var album in AlbumsSynced)
                {
                    var cursor = album.LocalPictures.GetEnumerator();
                    if (!cursor.MoveNext()) continue;

                    PicturesCursors.Add(cursor);
                }
            }

            while (piece.Count < PageSize)
            {
                IEnumerator<IPictureLocal>? addCursor = null;
                foreach (var currentCursor in PicturesCursors)
                {
                    if (addCursor == null)
                    {
                        addCursor = currentCursor;
                        continue;
                    }

                    if (addCursor    .Current.Date is DateTime  addDate &&
                        currentCursor.Current.Date is DateTime currDate &&
                        addDate < currDate
                    ) addCursor = currentCursor;
                }
                if (addCursor == null) break;

                piece.Add(addCursor.Current);

                if (!addCursor.MoveNext())
                {
                    PicturesCursors.Remove(addCursor);
                }
            }

            Debug.WriteLine($"LoadMore: piece: {JsonSerializer.Serialize(piece)}");

            foreach (var picture in piece)
            {
                string? currGroupTitle = picture.Date.ToString("MMMM, yyyy");
                string? lastGroupTitle = null;
        
                int lastGroupIndex = PicturesGroups.Count - 1;
                if (lastGroupIndex >= 0)
                    lastGroupTitle = PicturesGroups?[lastGroupIndex].Key;
        
                if (lastGroupTitle == currGroupTitle)
                    PicturesGroups[lastGroupIndex].Add(picture);
                else
                    PicturesGroups.Add(new(currGroupTitle ?? "", [picture]));
        
                //Debug.WriteLine($"Added: {picture.Name}, to: {currGroupTitle}");
            }

            CanLoadMore = piece.Any();
            Debug.WriteLine("=========== END LoadMore ============");
        }
        catch (Exception ex)
        {
            CanLoadMore = false;
            _ = Shell.Current.DisplayAlert("DEBUG", ex.Message, "OK");
            Debug.WriteLine($"LoadMore: Ex:\n{ex.Message}");
        }
    }
    */

    [ObservableProperty]
    private int _columnCount = 1;

    [ObservableProperty]
    private double? _requestColumnWidth = 120;

    [ObservableProperty]
    private double _columnWidth = 100;

    [RelayCommand]
    private void CalculateColumnsWidth(double containerWidth)
    {
        if (RequestColumnWidth != null)
            ColumnCount = Math.Max((int)(containerWidth / RequestColumnWidth), 1);

        ColumnWidth = containerWidth / ColumnCount;
        Debug.WriteLine($"=== ChgColW ===\n{ColumnWidth} = {containerWidth} / {ColumnCount}");
    }
}

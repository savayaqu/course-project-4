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

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string? error;

    [ObservableProperty]
    bool hasSynced = false;

    [ObservableProperty]
    private ObservableCollection<ItemsGroup<IPictureLocal>> picturesGroups = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalCount))]
    [NotifyPropertyChangedFor(nameof(SyncedCount))]
    private ObservableCollection<AlbumSynced> albumsSynced = [];

    public MainPageViewModel()
    {
        RefreshCommand.Execute(null);
    }

    public int LocalCount  => AlbumsSynced.Sum(a => a.TrueLocalPicturesCount);

    public int SyncedCount => AlbumsSynced.Sum(a => a.SyncedPicturesCount);

    public int PageSize => ColumnCount * 10;


    [RelayCommand]
    public async Task Refresh()
    {
        await RequestLocalAlbums();

        HasSynced = AlbumsSynced.Any();
        if (!HasSynced) return;

        PicturesGroups.Clear();
        PicturesCursors = null;
        CanLoadMore = true;

        await LoadMoreCommand.ExecuteAsync(null);
    }


    [RelayCommand]
    public async Task SyncManage()
    {
        GeneralSyncManagePopup popup = new();
        Shell.Current.CurrentPage.ShowPopup(popup);
    }


    [ObservableProperty]
    private bool hasPermissions = false;


    [RelayCommand]
    public async Task GoToAlbums()
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

    public async Task RequestLocalAlbums()
    {
        // Если не разрешено или не поддерживается — произойдёт исключение
        try
        {
            HasPermissions = await LocalData.CheckPermissions();
            if (!HasPermissions)
                return;
        }
        catch (PlatformNotSupportedException ex)
        {
            return;
        }

        if (LocalData.Status == LocalLoadStatus.NotLoad)
        {
            await LocalData.FillPictures();
        }
        else if (LocalData.Status == LocalLoadStatus.InLoad)
        {
            // TODO: непроверенный код
            _ = Shell.Current.DisplayAlert("Страшилка", "TODO: непроверенный код", "OK");
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
    }


    public List<IEnumerator<IPictureLocal>>? PicturesCursors = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadMoreCommand))]
    public bool canLoadMore = false;

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
                    lastGroupTitle = PicturesGroups?[lastGroupIndex].Title;
        
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


    [ObservableProperty]
    private int columnCount = 1;

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
    private async Task OpenViewer(Models.Pictures.IPicture picture)
    {
        await Shell.Current.Navigation.PushAsync(new ViewerPage(picture), false);
    }
}

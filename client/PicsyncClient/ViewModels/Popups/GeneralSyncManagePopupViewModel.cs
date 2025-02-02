using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Response;
using PicsyncClient.Models.Request;
using PicsyncClient.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;
using static PicsyncClient.Utils.LocalDB;
using PicsyncClient.Components.Popups;
using PicsyncClient.Models.Pictures;
using PicsyncClient.Models;

namespace PicsyncClient.ViewModels.Popups;

public partial class GeneralSyncManagePopupViewModel : ObservableObject
{
    [ObservableProperty] private string? error;
    [ObservableProperty] private bool manageIsVisible = false;

    public PictureSender Sender => PictureSender.Default;

    public ObservableCollection<ItemsKeyGroup<UploadsAlbum, UploadItem<PictureLocal>>> UploadsGroups { get; private set; } = [];

    private readonly Popup _popup;

    public GeneralSyncManagePopupViewModel(Popup popup)
    {
        _popup = popup;

        foreach (var uploadsAlbum in Sender.UploadsAlbums)
        {
            UploadsGroups.Add(new(uploadsAlbum, uploadsAlbum.Uploads));
        }

        ManageIsVisible = UploadsGroups.Any();
    }

    [RelayCommand]
    public void StartManualSync()
    {
        Sender.AllManualAlbumUpload();
        Debug.WriteLine($"StartManualSync: Sender.UploadsAlbums.Count {Sender.UploadsAlbums.Count}");
        
        foreach (var uploadsAlbum in Sender.UploadsAlbums)
        {
            if (UploadsGroups.Where(ug => ug.Key == uploadsAlbum).Any()) continue;

            UploadsGroups.Add(new(uploadsAlbum, uploadsAlbum.Uploads));
        }
        ManageIsVisible = UploadsGroups.Any();

        Sender.StartUploadIfNotActive();
    }

    [RelayCommand]
    public void StopOrCancelUpload()
    {
        if (Sender.IsUploading)
        {
            Sender.StopUpload();
            return;
        }
        UploadsGroups.Clear();
        ManageIsVisible = UploadsGroups.Any();
        Sender.ClearQueue();
    }

    [RelayCommand]
    public void Close()
    {
        _popup.Close();
    }
}

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
using System.Windows.Input;

namespace PicsyncClient.ViewModels.Popups;

public partial class InvitationCreatePopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private string? error;
    [ObservableProperty] private AlbumRemote album;
    [ObservableProperty] private int? joinLimit = 1;

    private bool isUpdating = false;

    private int? timeLimit = 10;
    public int? TimeLimit
    {
        get => timeLimit;
        set
        {
            timeLimit = value;
            if (isUpdating) return;

            isUpdating = true;

            if (value != null)
                expiresAt = DateTime.Now.AddMinutes((double)value);
            else
                expiresAt = DateTime.Now.AddMinutes(10);

            OnPropertyChanged(nameof(ExpiresAt));
            OnPropertyChanged(nameof(ExpiresAtDate));
            OnPropertyChanged(nameof(ExpiresAtTime));
            OnPropertyChanged(nameof(TimeLimit));

            isUpdating = false;
        }
    }

    private DateTime expiresAt;
    public DateTime ExpiresAt
    {
        get => expiresAt;
        set
        {
            expiresAt = value;
            if (isUpdating) return;

            isUpdating = true;

            OnPropertyChanged(nameof(ExpiresAt));
            OnPropertyChanged(nameof(ExpiresAtDate));
            OnPropertyChanged(nameof(ExpiresAtTime));

            isUpdating = false;
        }
    }

    public DateTime ExpiresAtDate
    {
        get => ExpiresAt.Date;
        set
        {
            expiresAt = value.Date + ExpiresAt.TimeOfDay;
            if (isUpdating) return;

            isUpdating = true;

            var minutes = (ExpiresAt - DateTime.Now).TotalMinutes;
            timeLimit = (int)Math.Round(minutes);

            OnPropertyChanged(nameof(ExpiresAt));
            OnPropertyChanged(nameof(ExpiresAtDate));
            OnPropertyChanged(nameof(TimeLimit));

            isUpdating = false;
        }
    }

    public TimeSpan ExpiresAtTime
    {
        get => ExpiresAt.TimeOfDay;
        set
        {
            expiresAt = ExpiresAt.Date + value;
            if (isUpdating) return;

            isUpdating = true;

            var minutes = (ExpiresAt - DateTime.Now).TotalMinutes;
            timeLimit = (int)Math.Round(minutes);

            OnPropertyChanged(nameof(ExpiresAt));
            OnPropertyChanged(nameof(ExpiresAtTime));
            OnPropertyChanged(nameof(TimeLimit));

            isUpdating = false;
        }
    }

    private CancellationTokenSource _cancelTokenSource = new();
    private readonly Popup _popup;

    public InvitationCreatePopupViewModel(Popup popup, AlbumRemote album)
    {
        _popup = popup;
        Album = album;
    }

    public bool CanConfirm => !IsBusy;

    [RelayCommand]
    public async Task Confirm()
    {
        int? joinLimit;
        if (HasJoinLimit && JoinLimit > 0)
        {
            joinLimit = JoinLimit;
            Debug.WriteLine($"InvitationCreate: joinLimit = {JoinLimit}");
        }
        else
        {
            joinLimit = null;
            NoJoinLimit = true;
            Debug.WriteLine("InvitationCreate: NoJoinLimit = true");
        }

        InvationCreateRequest reqBody;
        if (IsExpiresAtDateTime)
        {
            DateTime expiresAt = ExpiresAtDate.Date + ExpiresAtTime;
            reqBody = new(joinLimit, expiresAt);
            Debug.WriteLine("InvitationCreate: expiresAt: " + expiresAt);
        }
        else if (IsExpiresAfterTime && TimeLimit > 0)
        {
            reqBody = new(joinLimit, TimeLimit);
        }
        else
        {
            IsExpiresNever = true;
            reqBody = new(joinLimit);
        }
        Debug.WriteLine("InvitationCreate: reqBody: " + JsonSerializer.Serialize(reqBody));

        (var res, var body) = await FetchAsync<InvitationResponse>(
            HttpMethod.Post, URLs.AlbumInvite(Album.Id),
            f => IsBusy = f, e => Error = e,
            reqBody,
            cancellationToken: _cancelTokenSource.Token,
            serialize: true
        );

        if (body == null) return;

        _popup.Close(body.Invitation);
    }

    [RelayCommand]
    public void Cancel()
    {
        _cancelTokenSource.Cancel();
        _popup.Close();
    }

    [ObservableProperty] private bool isExpiresNever = true;
    [ObservableProperty] private bool isExpiresAfterTime;
    [ObservableProperty] private bool isExpiresAtDateTime;
    [ObservableProperty] private bool noJoinLimit = true;
    [ObservableProperty] private bool hasJoinLimit;

    [RelayCommand] public void SelectExpiresNever     ()  => IsExpiresNever      = true;
    [RelayCommand] public void SelectExpiresAfterTime ()  => IsExpiresAfterTime  = true;
    [RelayCommand] public void SelectExpiresAtDateTime()  => IsExpiresAtDateTime = true;
    [RelayCommand] public void SelectNoJoinLimit      ()  => NoJoinLimit         = true;
    [RelayCommand] public void SelectHasJoinLimit     ()  => HasJoinLimit        = true;
}

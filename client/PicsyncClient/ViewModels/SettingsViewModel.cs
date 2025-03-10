using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Components.Popups;
using PicsyncClient.Models;
using PicsyncClient.Utils;

namespace PicsyncClient.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryExitCommand))]
    private bool _isFetch ;

    [ObservableProperty] private string? _statsGetError;
    [ObservableProperty] private string? _settingsGetError;

    public User?           User           => AuthData.User;
    public UserStats?      Stats          => AuthData.Stats;
    public Uri?            Url            => ServerData.Url;
    public ServerSettings? ServerSettings => ServerData.Settings;

    public string AllowedUploadMimes => String.Join(" ", ServerSettings?.AllowedUploadMimes ?? []);
    public string AllowedPreviewSize => String.Join(" ", ServerSettings?.AllowedPreviewSizes ?? []);


    [RelayCommand(IncludeCancelCommand = true)]
    private async Task Refresh(CancellationToken token = default)
    {
        var   userTask =   AuthData.Update(e =>    StatsGetError = e, token);
        var serverTask = ServerData.Update(e => SettingsGetError = e, token);

        await Task.WhenAll(userTask, serverTask);

        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(Stats));
        OnPropertyChanged(nameof(Url));
        OnPropertyChanged(nameof(ServerSettings));
    }

    public void SoftUpdate()
    {
        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(Stats));
        OnPropertyChanged(nameof(Url));
        OnPropertyChanged(nameof(ServerSettings));
    }


    [RelayCommand]
    private async Task CopyUrl()
    {
        if (Url is not { } uri) return;
        await Clipboard.SetTextAsync(uri.ToString());
        await Toast.Make("Скопировано в буфер").Show();
    }


    [RelayCommand]
    private async Task EditSelf()
    {
        EditSelfPopup popup = new();
        await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        OnPropertyChanged(nameof(User));
    }

    private bool CanExit() => !IsFetch;

    [RelayCommand(CanExecute = nameof(CanExit))]
    private async Task TryExit()
    {
        await AuthData.TryExitAndNavigate(isFetch => IsFetch = isFetch);
    }
}

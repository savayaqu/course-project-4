using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models;
using PicsyncClient.Models.Response;
using PicsyncClient.Utils;

namespace PicsyncClient.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryExitCommand))]
    private bool isFetch = false;

    [ObservableProperty] private string? statsGetError;
    [ObservableProperty] private string? settingsGetError;

    public User? User => AuthData.User;
    public UserStats? Stats => AuthData.Stats;
    public Uri? Url => ServerData.Url;
    public ServerSettings? ServerSettings => ServerData.Settings;

    public string AllowedUploadMimes => String.Join(" ", ServerSettings?.AllowedUploadMimes ?? []);
    public string AllowedPreviewSize => String.Join(" ", ServerSettings?.AllowedPreviewSizes ?? []);


    [RelayCommand(IncludeCancelCommand = true)]
    private async Task Refresh(CancellationToken token = default)
    {
        var   userTask =   AuthData.Update(_ =>    StatsGetError = _, token);
        var serverTask = ServerData.Update(_ => SettingsGetError = _, token);

        await Task.WhenAll(userTask, serverTask);

        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(Stats));
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
        if (Url is not Uri uri) return;
        await Clipboard.SetTextAsync(uri.ToString());
        await Toast.Make("Скопировано в буфер").Show();
    }

    private bool CanExit() => !IsFetch;

    [RelayCommand(CanExecute = nameof(CanExit))]
    private async Task TryExit()
    {
        await AuthData.TryExitAndNavigate(isFetch => IsFetch = isFetch);
    }
}

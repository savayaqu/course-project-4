using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Utils;

namespace PicsyncClient.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryExitCommand))]
    private bool isFetch = false;

    [RelayCommand(CanExecute = nameof(CanExit))]
    private async Task TryExit()
    {
        await AuthData.TryExitAndNavigate(isFetch => IsFetch = isFetch);
    }
    private bool CanExit() => !IsFetch;
}

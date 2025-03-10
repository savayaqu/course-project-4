using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Utils;
using System.Collections.ObjectModel;

namespace PicsyncClient.ViewModels.Auth;

public partial class ServerSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryNewConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TryPastConnectCommand))]
    private string _url = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryNewConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TryPastConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectCommand))]
    private bool _isFetch;

    [ObservableProperty]
    private string? _error;

    private CancellationTokenSource? _cancellationTokenSource;

    public ObservableCollection<string> PastUrls => ServerData.PastUrls;

    public bool PastUrlsIsVisible => PastUrls.Count > 0;

    public ServerSelectorViewModel()
    {
        PastUrls.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(PastUrlsIsVisible));
    }

    public bool CanTryNewConnect() => Url != "" && !IsFetch;

    [RelayCommand(CanExecute = nameof(CanTryNewConnect))]
    private async Task TryNewConnect()
    {
        _cancellationTokenSource = new();
        bool isSuccess = await ServerData.TrySaveAndNavigate(
            Url,
            isFetch => IsFetch = isFetch,
            error => Error = error,
            _cancellationTokenSource.Token
        );
        if (isSuccess)
        {
            Url = "";
        }
    }

    private bool CanTryPastConnect() => !IsFetch;

    [RelayCommand(CanExecute = nameof(CanTryPastConnect))]
    private async Task TryPastConnect(string url)
    {
        _cancellationTokenSource = new();
        await ServerData.TrySaveAndNavigate(
            url, 
            isFetch => IsFetch = isFetch,
            error => Error = error,
            _cancellationTokenSource.Token
        );
    }

    public bool CanCancelConnect() => IsFetch;

    [RelayCommand(CanExecute = nameof(CanCancelConnect))]
    private void CancelConnect()
    {
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void DeletePastUrl(string url) =>
        PastUrls.Remove(url);
}

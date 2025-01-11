using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Utils;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PicsyncClient.ViewModels.Auth;

public partial class ServerSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryNewConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TryPastConnectCommand))]
    private string url = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryNewConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TryPastConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectCommand))]
    private bool isFetch = false;

    [ObservableProperty]
    private string? error = null;

    private CancellationTokenSource _cancellationTokenSource;

    public ObservableCollection<string> PastUrls => ServerData.PastUrls;

    public bool PastUrlsIsVisible => PastUrls.Count > 0;

    public ServerSelectorViewModel()
    {
        PastUrls.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) 
            => OnPropertyChanged(nameof(PastUrlsIsVisible));
    }

    public bool CanTryNewConnect() => url != "" && !IsFetch;

    [RelayCommand(CanExecute = nameof(CanTryNewConnect))]
    private async Task TryNewConnect()
    {
        _cancellationTokenSource = new();
        bool isSucces = await ServerData.TrySaveAndNavigate(
            Url,
            isFetch => IsFetch = isFetch,
            error => Error = error,
            _cancellationTokenSource.Token
        );
        if (isSucces)
        {
            Url = "";
        }
    }

    private bool CanTryPastConnect() => !IsFetch;

    [RelayCommand(CanExecute = nameof(CanTryPastConnect))]
    private async Task TryPastConnect(string url)
    {
        _cancellationTokenSource = new();
        bool isSucces = await ServerData.TrySaveAndNavigate(
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
    private async Task DeletePastUrl(string url) =>
        PastUrls.Remove(url);
}

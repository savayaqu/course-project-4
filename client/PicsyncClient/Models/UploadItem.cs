using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace PicsyncClient.Models;

public partial class UploadItem<TItem>() : ObservableObject
{
    [SetsRequiredMembers]
    public UploadItem(TItem item) : this()
    {
        Item = item;
    }

    public required TItem Item { get; init; }

    [ObservableProperty] private string?   _error;
    [ObservableProperty] private DateTime? _startedAt;
    [ObservableProperty] private TimeSpan? _timeSpent;
    [ObservableProperty] private double    _progress  = 0;
}
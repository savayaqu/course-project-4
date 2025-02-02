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

    public required TItem Item { get; set; }

    [ObservableProperty]
    public string? error;

    [ObservableProperty]
    public DateTime? startedAt;

    [ObservableProperty]
    public TimeSpan? timeSpent;

    [ObservableProperty]
    public double  progress  = 0;
}
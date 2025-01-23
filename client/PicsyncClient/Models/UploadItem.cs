using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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
    public double  progress  = 0;
}
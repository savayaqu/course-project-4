using CommunityToolkit.Mvvm.ComponentModel;
using PicsyncClient.Models.Pictures;

namespace PicsyncClient.ViewModels;

public partial class ViewerViewModel(Models.Pictures.IPicture picture) : ObservableObject
{
    [ObservableProperty]
    private bool isBusy = false;

    [ObservableProperty]
    private Models.Pictures.IPicture picture = picture;
}

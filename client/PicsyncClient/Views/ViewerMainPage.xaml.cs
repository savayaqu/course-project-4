using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class ViewerMainPage : ContentPage
{
    private ViewerMainViewModel _viewModel;
    private Action<Models.Pictures.IPicture>? _scrollTo;

    public ViewerMainPage(
        Models.Pictures.IPicture picture, 
        List<Models.Pictures.IPicture>? listPictures = null,
        Action<Models.Pictures.IPicture>? scrollTo = null
    ) {
		InitializeComponent();
        BindingContext = _viewModel = new ViewerMainViewModel(picture, listPictures);
        _scrollTo = scrollTo;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _scrollTo?.Invoke(_viewModel.Picture);
#if ANDROID
        var activity = Platform.CurrentActivity;
        System.Diagnostics.Debug.WriteLine($"activity dis: {activity}");
        if (activity == null
         || activity.Window == null
         || activity.Window.DecorView == null) return;

        activity.Window.DecorView.SystemUiVisibility = Android.Views.StatusBarVisibility.Visible;
#elif IOS
        UIKit.UIApplication.SharedApplication.SetStatusBarHidden(false, UIKit.UIStatusBarAnimation.Fade);
#endif
    }
}
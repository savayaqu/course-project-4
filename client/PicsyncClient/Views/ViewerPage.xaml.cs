using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class ViewerPage : ContentPage
{
	public ViewerPage(Models.Pictures.IPicture picture, AlbumViewModel? albumViewModel = null)
	{
		InitializeComponent();
        BindingContext = new ViewerViewModel(picture, albumViewModel);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

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
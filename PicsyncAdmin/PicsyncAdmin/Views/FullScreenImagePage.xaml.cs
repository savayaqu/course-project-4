using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class FullScreenImagePage : ContentPage
{
	public FullScreenImagePage(Picture picture, ulong albumId)
	{
		InitializeComponent();
        BindingContext = new FullScreenImageViewModel(picture, albumId);

    }
}
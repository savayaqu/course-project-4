using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class EditSelfPopup : Popup
{
	public EditSelfPopup()
	{
		InitializeComponent();
		BindingContext = new EditSelfPopupViewModel(this);
    }
}
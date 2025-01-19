using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class InvitationPreviewPopup : Popup
{
	public InvitationPreviewPopup()
	{
		InitializeComponent();
		BindingContext = new InvitationPreviewPopupViewModel(this);
    }

    private void CollectionView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is not CollectionView collection ||
            BindingContext is not InvitationPreviewPopupViewModel viewModel) return;

        viewModel.CalculateColumnsWidthCommand.Execute(collection.Width);
    }
}
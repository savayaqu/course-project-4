using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class ApiUrlSelectionPage : ContentPage
{
	public ApiUrlSelectionPage()
	{
		InitializeComponent();
        BindingContext = new ApiUrlSelectionViewModel();

    }
}
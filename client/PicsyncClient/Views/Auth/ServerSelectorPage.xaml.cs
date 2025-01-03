using PicsyncClient.ViewModels.Auth;

namespace PicsyncClient.Views.Auth;

public partial class ServerSelectorPage : ContentPage
{
	public ServerSelectorPage()
	{
		InitializeComponent();
		BindingContext = new ServerSelectorViewModel();
    }

    private void TryNewConnect(object sender, EventArgs e)
    {
		if (BindingContext is ServerSelectorViewModel vm) 
			vm.TryNewConnectCommand.Execute(null);
    }
}
using AdminApi.Models;

namespace AdminApi.Views;

public partial class Home : ContentPage
{
	private User _user;
	private string _token;
	public Home(User user, string token)
	{
		InitializeComponent();
		_user = user;
		_token = token;

		//Устанавливаем аватар и никнейм
		LoginHome.Text = _user.Login;

    }
}
using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.Views.Auth;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PicsyncAdmin.Views;

public partial class Home : ContentPage
{
	private User _user;
	private string _token;
    public ObservableCollection<Complaint> Complaints { get; set; } = new();
    private readonly HttpClient _httpClient = new HttpClient();

    public Home(User user, string token)
	{
		InitializeComponent();
		_user = user;
		_token = token;

        // Привязка данных к списку
        BindingContext = this;
        // Загружаем жалобы
        LoadComplaints();
    }

    private async void LoadComplaints()
    {
        var complaints = await MethodComplaint.LoadComplaints(_token);

        if (complaints.Any())
        {
            // Assign the complaints to the ObservableCollection
            Complaints.Clear();
            foreach (var complaint in complaints)
            {
                Complaints.Add(complaint);
                Debug.WriteLine(complaint);
            }
            // For debugging, you can log the number of complaints
            Debug.WriteLine($"Received {complaints.Count} complaints");
        }
        else
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить жалобы", "OK");
        }
    }



    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        bool response = await MethodLogout.Logout(_user, _token);
        if (response == true)
        {
            await Navigation.PushAsync(new Login());
        }
        else
        {
            await DisplayAlert("Ошибка выхода", "Пиздец", "ОК");
        }
    }
            
}

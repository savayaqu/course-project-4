using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;


public partial class UserContentPage : ContentPage
{
    private UserContentViewModel ViewModel => (UserContentViewModel)BindingContext;


    public UserContentPage(Complaint complaint)
    {
        InitializeComponent();
        BindingContext = new UserContentViewModel(complaint); // ����� �������� ��������� ViewModel
        SizeChanged += OnSizeChanged;

    }
    private void OnSizeChanged(object sender, EventArgs e)
    {
        // ������������ ���������� �������� �� ������ ������ ������
        if (AdaptiveGridLayout != null && Width > 0)
        {
            int columnCount = (int)(Width / 150); // ������ ������ ������� 150 ��������
            columnCount = Math.Max(columnCount, 1); // ������� 1 �������
            AdaptiveGridLayout.Span = columnCount;
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadData();
    }
}



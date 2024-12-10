using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;


public partial class UserContentPage : ContentPage
{
    private UserContentViewModel ViewModel => (UserContentViewModel)BindingContext;


    public UserContentPage(Complaint complaint)
    {
        InitializeComponent();
        BindingContext = new UserContentViewModel(complaint); // Здесь создаётся экземпляр ViewModel
        SizeChanged += OnSizeChanged;

    }
    private void OnSizeChanged(object sender, EventArgs e)
    {
        // Рассчитываем количество столбцов на основе ширины экрана
        if (AdaptiveGridLayout != null && Width > 0)
        {
            int columnCount = (int)(Width / 150); // Ширина каждой колонки 150 пикселей
            columnCount = Math.Max(columnCount, 1); // Минимум 1 колонка
            AdaptiveGridLayout.Span = columnCount;
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadData();
    }
}



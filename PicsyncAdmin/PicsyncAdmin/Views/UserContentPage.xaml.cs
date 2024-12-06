using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

[QueryProperty(nameof(AlbumId), "albumId")]
[QueryProperty(nameof(PicturePath), "picturePath")]
public partial class UserContentPage : ContentPage
{
    private UserContentPageViewModel ViewModel => (UserContentPageViewModel)BindingContext;

    public ulong AlbumId
    {
        get => ViewModel.AlbumId;
        set => ViewModel.AlbumId = value;
    }

    public string? PicturePath
    {
        get => ViewModel.PicturePath;
        set => ViewModel.PicturePath = value;
    }

    public UserContentPage()
    {
        InitializeComponent();
        BindingContext = new UserContentPageViewModel(new HttpClient()); // ����� �������� ��������� ViewModel
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
        await ViewModel.LoadDataAsync();
    }
}



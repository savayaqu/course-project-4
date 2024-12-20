namespace PicsyncClient.Views;

public partial class AlbumsPage : ContentPage
{
	public AlbumsPage()
	{
		InitializeComponent();
        SizeChanged += OnSizeChanged;
    }
    private void OnSizeChanged(object sender, EventArgs e)
    {
        // ������������ ���������� �������� �� ������ ������ ������
        if (AdaptiveGridLayout != null && Width > 0)
        {
            int columnCount = (int)(Width / 298);
            columnCount = Math.Max(columnCount, 1);
            AdaptiveGridLayout.Span = columnCount;
        }
    }
}
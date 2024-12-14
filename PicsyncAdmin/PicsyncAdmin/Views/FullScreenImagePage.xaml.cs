using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class FullScreenImagePage : ContentPage
{
    private double _currentScale = 1;      // ������� �������
    private double _startScale = 1;       // ������� � ������ �����
    private double _xOffset = 0;          // �������� �� X
    private double _yOffset = 0;          // �������� �� Y
    private double _startX = 0;           // ��������� ������� X
    private double _startY = 0;           // ��������� ������� Y
    private const double MinScale = 1;    // ����������� �������
    private const double MaxScale = 5;    // ������������ �������

    private bool _isPinching = false;     // ���� ��� ����������� ��������� ����� Pinch
    private bool _isPanning = false;      // ���� ��� ����������� ��������� ����� Pan
    public FullScreenImagePage(Picture picture, ulong albumId)
	{
		InitializeComponent();
        BindingContext = new FullScreenImageViewModel(picture, albumId);
        //TODO: ������ �������� ����������� � ��������

    }
    // ��������� ����� ���������������
    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _isPinching = true;  // �������� ���� Pinch
            _startScale = _currentScale;
        }
        else if (e.Status == GestureStatus.Running)
        {
            // ���� ���� ���������������, ��������� ������ Pan-��������
            _isPanning = false;

            // ��������� ����� �������
            _currentScale = Math.Max(MinScale, Math.Min(MaxScale, _startScale * e.Scale));
            ZoomableImage.Scale = _currentScale;
        }
        else if (e.Status == GestureStatus.Completed)
        {
            _isPinching = false;  // ��������� Pinch
        }
    }

    // ��������� ����� �����������
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_currentScale <= 1 || _isPinching)
            return; // ����������� �� ��������� ��� �������� <= 1 ��� ����� ���� Pinch

        if (e.StatusType == GestureStatus.Started)
        {
            _isPanning = true;  // �������� Pan-��������
            _startX = e.TotalX - _xOffset;
            _startY = e.TotalY - _yOffset;
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            // ��������� ����� ��������
            _xOffset = e.TotalX - _startX;
            _yOffset = e.TotalY - _startY;

            // ��������� �������� � ������ ������
            ZoomableImage.TranslationX = Clamp(_xOffset, -ZoomableImage.Width * (_currentScale - 1) / 2, ZoomableImage.Width * (_currentScale - 1) / 2);
            ZoomableImage.TranslationY = Clamp(_yOffset, -ZoomableImage.Height * (_currentScale - 1) / 2, ZoomableImage.Height * (_currentScale - 1) / 2);
        }
        else if (e.StatusType == GestureStatus.Completed)
        {
            _isPanning = false;  // ��������� Pan
        }
    }

    // ����� ��� ����������� �������� (������)
    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }


}
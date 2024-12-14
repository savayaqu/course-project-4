using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class FullScreenImagePage : ContentPage
{
    private double _currentScale = 1;      // Текущий масштаб
    private double _startScale = 1;       // Масштаб в начале жеста
    private double _xOffset = 0;          // Смещение по X
    private double _yOffset = 0;          // Смещение по Y
    private double _startX = 0;           // Начальная позиция X
    private double _startY = 0;           // Начальная позиция Y
    private const double MinScale = 1;    // Минимальный масштаб
    private const double MaxScale = 5;    // Максимальный масштаб

    private bool _isPinching = false;     // Флаг для определения активного жеста Pinch
    private bool _isPanning = false;      // Флаг для определения активного жеста Pan
    public FullScreenImagePage(Picture picture, ulong albumId)
	{
		InitializeComponent();
        BindingContext = new FullScreenImageViewModel(picture, albumId);
        //TODO: коряво работает перемещение и скейлинг

    }
    // Обработка жеста масштабирования
    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _isPinching = true;  // Начинаем жест Pinch
            _startScale = _currentScale;
        }
        else if (e.Status == GestureStatus.Running)
        {
            // Если идет масштабирование, отключаем ложные Pan-движения
            _isPanning = false;

            // Вычисляем новый масштаб
            _currentScale = Math.Max(MinScale, Math.Min(MaxScale, _startScale * e.Scale));
            ZoomableImage.Scale = _currentScale;
        }
        else if (e.Status == GestureStatus.Completed)
        {
            _isPinching = false;  // Завершаем Pinch
        }
    }

    // Обработка жеста перемещения
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_currentScale <= 1 || _isPinching)
            return; // Перемещение не требуется при масштабе <= 1 или когда идет Pinch

        if (e.StatusType == GestureStatus.Started)
        {
            _isPanning = true;  // Начинаем Pan-движение
            _startX = e.TotalX - _xOffset;
            _startY = e.TotalY - _yOffset;
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            // Вычисляем новое смещение
            _xOffset = e.TotalX - _startX;
            _yOffset = e.TotalY - _startY;

            // Применяем смещение с учетом границ
            ZoomableImage.TranslationX = Clamp(_xOffset, -ZoomableImage.Width * (_currentScale - 1) / 2, ZoomableImage.Width * (_currentScale - 1) / 2);
            ZoomableImage.TranslationY = Clamp(_yOffset, -ZoomableImage.Height * (_currentScale - 1) / 2, ZoomableImage.Height * (_currentScale - 1) / 2);
        }
        else if (e.StatusType == GestureStatus.Completed)
        {
            _isPanning = false;  // Завершаем Pan
        }
    }

    // Метод для ограничения значений (сдвига)
    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }


}
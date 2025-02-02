// ----------------------------------------------------------------------------
// Bertuzzi.MAUI.PinchZoomImage
// https://www.nuget.org/packages/Bertuzzi.MAUI.PinchZoomImage/
// 
// This software is licensed under the Microsoft Public License (Ms-PL).
// Full license text can be found at: https://opensource.org/licenses/MS-PL
// 
// Copyright (c) 2022 Bertuzzi
// ----------------------------------------------------------------------------
// Modified by savayaqu

using System.Diagnostics;
using System.Windows.Input;

namespace Bertuzzi.MAUI.PinchZoomImage
{
    public class PinchZoom : ContentView
    {
        private double _currentScale = 1;
        private double _startScale = 1;
        private double _xOffset = 0;
        private double _yOffset = 0; 
        
        public static readonly BindableProperty TappedCommandProperty =
            BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(PinchZoom), null);

        public ICommand? TappedCommand
        {
            get => (ICommand)GetValue(TappedCommandProperty);
            set => SetValue(TappedCommandProperty, value);
        }

        public PinchZoom()
        {
            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += PinchUpdated;
            GestureRecognizers.Add(pinchGesture);

            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);

            var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTapGesture.Tapped += DoubleTapped;
            GestureRecognizers.Add(doubleTapGesture);

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => TappedCommand?.Execute(null);
            GestureRecognizers.Add(tapGesture);
        }
        private void PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _startScale = Content.Scale;
                    Content.AnchorX = 0;
                    Content.AnchorY = 0;
                    break;
                case GestureStatus.Running:
                    {
                        _currentScale += (e.Scale - 1) * _startScale;
                        _currentScale = Math.Max(1, _currentScale);

                        var renderedX = Content.X + _xOffset;
                        var deltaX = renderedX / Width;
                        var deltaWidth = Width / (Content.Width * _startScale);
                        var originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                        var renderedY = Content.Y + _yOffset;
                        var deltaY = renderedY / Height;
                        var deltaHeight = Height / (Content.Height * _startScale);
                        var originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                        var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                        var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                        Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
                        Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

                        Content.Scale = _currentScale;
                        Debug.WriteLine(_currentScale);
                        break;
                    }
                case GestureStatus.Completed:
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
            }
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Content.Scale == 1)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    var newX = (e.TotalX * Scale) + _xOffset;
                    var newY = (e.TotalY * Scale) + _yOffset;

                    // Получаем размеры видимой области (вашего ContentView или страницы)
                    var imageWidth = this.Width;
                    var imageHeight = this.Height;
                    var DisplayMaxHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
                    var DisplayMaxWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;

                    Debug.WriteLine("DisplayMaxHeight: " + DisplayMaxHeight);
                    Debug.WriteLine("DisplayMaxWidth: " + DisplayMaxWidth);

                    // Получаем размеры изображения с учетом масштаба
                    var scaledWidth = Content.Width * Content.Scale;
                    var scaledHeight = Content.Height * Content.Scale;

                    // Логируем ключевые переменные
                    Debug.WriteLine($"imageWidth: {imageWidth}, imageHeight: {imageHeight}");
                    Debug.WriteLine($"scaledWidth: {scaledWidth}, scaledHeight: {scaledHeight}");
                    Debug.WriteLine($"newX: {newX}, newY: {newY}");

                    // Проверяем, можно ли перемещать изображение по горизонтали и вертикали
                    var canMoveX = scaledWidth > imageWidth && DisplayMaxWidth <= scaledWidth;
                    var canMoveY = scaledHeight > imageHeight && DisplayMaxHeight <= scaledHeight;

                    Debug.WriteLine($"canMoveX: {canMoveX}, canMoveY: {canMoveY}");
                    var emptyWidth = (DisplayMaxWidth - imageWidth) / 2;
                    var emptyHeight = (DisplayMaxHeight - imageHeight) / 2;

                    // Ограничение по горизонтали (X)
                    if (canMoveX)
                    {
                        var minX = imageWidth - scaledWidth + emptyWidth; // Левая граница
                        var maxX = 0 - emptyWidth; // Правая граница

                        if (newX < minX)
                        {
                            newX = minX;
                        }

                        if (newX > maxX)
                        {
                            newX = maxX;
                        }
                    }
                    else
                    {
                        // Если изображение меньше видимой области, центрируем его
                        newX = (imageWidth - scaledWidth) / 2;
                    }
                    // Ограничение по вертикали (Y)
                    if (canMoveY)
                    {
                        // Если изображение больше видимой области, разрешаем перемещение с ограничениями
                        var minY = imageHeight - scaledHeight + emptyHeight; // Верхняя граница
                        var maxY = 0 - emptyHeight; // Нижняя граница

                        Debug.WriteLine($"minY: {minY}, maxY: {maxY}");

                        if (newY < minY)
                        {
                            newY = minY;
                        }

                        if (newY > maxY)
                        {
                            newY = maxY;
                        }
                    }
                    else
                    {
                        // Если перемещение по вертикали запрещено, центрируем изображение по вертикали
                        Debug.WriteLine("Перемещение по вертикали запрещено. Центрируем изображение по вертикали.");
                        newY = (imageHeight - scaledHeight) / 2; // Центрируем изображение
                    }

                    Debug.WriteLine($"Итоговые значения: newX: {newX}, newY: {newY}");

                    // Применяем новые координаты
                    Content.TranslationX = newX;
                    Content.TranslationY = newY;
                    break;

                case GestureStatus.Completed:
                    // Сохраняем текущее смещение для следующего перемещения
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;

                case GestureStatus.Started:
                    break;

                case GestureStatus.Canceled:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public async void DoubleTapped(object sender, EventArgs e)
        {
            var multiplicator = 2;
            _startScale = Content.Scale;
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            if (_currentScale == 1) // если это не второй двойной тап, увеличиваем масштаб
            {
                _currentScale *= multiplicator;
            }
            else // если это второй двойной тап, возвращаем масштаб к 1
            {
                _currentScale = 1; // Устанавливаем масштаб в 1
            }

            var renderedX = Content.X + _xOffset;
            var deltaX = renderedX / Width;
            var deltaWidth = Width / (Content.Width * _startScale);
            var originX = (0.5 - deltaX) * deltaWidth;

            var renderedY = Content.Y + _yOffset;
            var deltaY = renderedY / Height;
            var deltaHeight = Height / (Content.Height * _startScale);
            var originY = (0.5 - deltaY) * deltaHeight;

            var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
            var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

            Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
            Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

            Content.Scale = _currentScale;
            await Task.Delay(10);

            _xOffset = Content.TranslationX;
            _yOffset = Content.TranslationY;
        }
    }
}
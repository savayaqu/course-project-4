// ----------------------------------------------------------------------------
// Bertuzzi.MAUI.PinchZoomImage
// https://www.nuget.org/packages/Bertuzzi.MAUI.PinchZoomImage/
// 
// This software is licensed under the Microsoft Public License (Ms-PL).
// Full license text can be found at: https://opensource.org/licenses/MS-PL
// 
// Copyright (c) 2022 Bertuzzi
// ----------------------------------------------------------------------------

namespace Bertuzzi.MAUI.PinchZoomImage
{
    public class PinchZoom : ContentView
    {
        private double _currentScale = 1;
        private double _startScale = 1;
        private double _xOffset = 0;
        private double _yOffset = 0;
        private bool _secondDoubleTapp = false;

        public PinchZoom()
        {
            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += PinchUpdated;
            GestureRecognizers.Add(pinchGesture);

            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);

            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            tapGesture.Tapped += DoubleTapped;
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
            if (Content.Scale <= 1)
            {
                // Если масштаб 1 или меньше, не позволяем перемещать
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Рассчитываем новое положение
                    var newX = (e.TotalX / Scale) + _xOffset;
                    var newY = (e.TotalY / Scale) + _yOffset;

                    // Размеры видимой области
                    var scaledWidth = Content.Width * Content.Scale;
                    var scaledHeight = Content.Height * Content.Scale;

                    var screenWidth = Application.Current.MainPage.Width;
                    var screenHeight = Application.Current.MainPage.Height;

                    // Ограничение по горизонтали
                    var maxX = Math.Max(0, (scaledWidth - screenWidth) / 2);
                    var minX = -maxX;

                    newX = Math.Max(minX, Math.Min(maxX, newX));

                    // Ограничение по вертикали
                    var maxY = Math.Max(0, (scaledHeight - screenHeight) / 2);
                    var minY = -maxY;

                    newY = Math.Max(minY, Math.Min(maxY, newY));

                    // Применяем новое положение
                    Content.TranslationX = newX;
                    Content.TranslationY = newY;
                    break;

                case GestureStatus.Completed:
                    // Сохраняем текущее положение для последующих расчетов
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
            }
        }


        public async void DoubleTapped(object sender, EventArgs e)
        {
            var multiplicator = Math.Pow(2, 1.0 / 10.0);
            _startScale = Content.Scale;
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            for (var i = 0; i < 10; i++)
            {
                if (!_secondDoubleTapp) //if it's not the second double tapp we enlarge the scale
                {
                    _currentScale *= multiplicator;
                }
                else //if it's the second double tap we make the scale smaller again 
                {
                    _currentScale /= multiplicator;
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
            }
            _secondDoubleTapp = !_secondDoubleTapp;
            _xOffset = Content.TranslationX;
            _yOffset = Content.TranslationY;
        }
    }
}
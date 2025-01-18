using PicsyncAdmin.ViewModels;

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

            var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTapGesture.Tapped += DoubleTapped;
            GestureRecognizers.Add(doubleTapGesture);

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += Tapped;
            GestureRecognizers.Add(tapGesture);
        }
        private async void Tapped(object sender, EventArgs e)
        {
            UserContentViewModel.Instance.ToggleControlsVisibilityCommand.Execute(null);
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
                    var visibleWidth = this.Width;
                    var visibleHeight = this.Height;

                    // Получаем размеры изображения с учетом масштаба
                    var scaledWidth = Content.Width * Content.Scale;
                    var scaledHeight = Content.Height * Content.Scale;

                    // Проверяем, можно ли перемещать изображение по горизонтали и вертикали
                    var canMoveX = scaledWidth > visibleWidth;
                    var canMoveY = scaledHeight > visibleHeight;

                    if (canMoveX)
                    {
                        // Ограничиваем перемещение по горизонтали
                        var minX = visibleWidth - scaledWidth; // Левая граница
                        var maxX = 0; // Правая граница

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
                        newX = (visibleWidth - scaledWidth) / 2;
                    }

                    if (canMoveY)
                    {
                        // Ограничиваем перемещение по вертикали
                        var minY = visibleHeight - scaledHeight; // Верхняя граница
                        var maxY = 0; // Нижняя граница

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
                        // Если изображение меньше видимой области, центрируем его
                        newY = (visibleHeight - scaledHeight) / 2;
                    }

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
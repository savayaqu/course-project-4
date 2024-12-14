namespace PicsyncAdmin.Helpers
{
    public static class AppSettings
    {
        public static event Action? SettingsUpdated; // Событие для обновлений

        private static int _uploadDisablePercentage;
        public static int UploadDisablePercentage
        {
            get => _uploadDisablePercentage;
            set
            {
                if (_uploadDisablePercentage != value)
                {
                    _uploadDisablePercentage = value;
                    OnSettingsUpdated(); // Вызываем событие обновления
                }
            }
        }

        private static long _totalSpace;
        public static long TotalSpace
        {
            get => _totalSpace;
            set
            {
                if (_totalSpace != value)
                {
                    _totalSpace = value;
                    OnSettingsUpdated(); // Вызываем событие обновления
                }
            }
        }

        private static long _freeSpace;
        public static long FreeSpace
        {
            get => _freeSpace;
            set
            {
                if (_freeSpace != value)
                {
                    _freeSpace = value;
                    OnSettingsUpdated(); // Вызываем событие обновления
                }
            }
        }

        private static long _usedSpace;
        public static long UsedSpace
        {
            get => _usedSpace;
            set
            {
                if (_usedSpace != value)
                {
                    _usedSpace = value;
                    OnSettingsUpdated(); // Вызываем событие обновления
                }
            }
        }

        private static long _usedPercent;
        public static long UsedPercent
        {
            get => _usedPercent;
            set
            {
                if (_usedPercent != value)
                {
                    _usedPercent = value;
                    OnSettingsUpdated(); // Вызываем событие обновления
                }
            }
        }

        // Метод для вызова события
        private static void OnSettingsUpdated()
        {
            SettingsUpdated?.Invoke(); // Уведомляем подписчиков
        }

        // Метод для преобразования байтов в человекочитаемый формат
        public static string BytesToHuman(long bytes)
        {
            string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB" };

            if (bytes < 0)
            {
                return "0 B";
            }

            int pow = (int)Math.Floor(bytes > 0 ? Math.Log(bytes) / Math.Log(1024) : 0);
            pow = Math.Min(pow, units.Length - 1);

            double size = bytes / Math.Pow(1024, pow);
            return $"{size:0.###} {units[pow]}";
        }
    }
}

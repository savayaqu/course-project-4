using System;

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
                    SettingsUpdated?.Invoke(); // Сообщаем о новом значении
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
                    SettingsUpdated?.Invoke(); // Сообщаем о новом значении
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
                    SettingsUpdated?.Invoke(); // Сообщаем о новом значении
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
                    SettingsUpdated?.Invoke(); // Сообщаем о новом значении
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
                    SettingsUpdated?.Invoke(); // Сообщаем о новом значении
                }
            }
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

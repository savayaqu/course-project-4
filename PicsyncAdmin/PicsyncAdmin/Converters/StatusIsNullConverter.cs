﻿namespace PicsyncAdmin.Converters
{
    public class StatusIsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Проверяем, что значение статуса равно null
            if (value == null)
                return true; // Статус равен null
            return false; // Статус не равен null
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
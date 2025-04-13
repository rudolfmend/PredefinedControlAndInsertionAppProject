using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Converter that compares strings and returns Visibility.Visible if they match
    /// Konvertor, ktorý porovnáva reťazce a vracia Visibility.Visible, ak sa zhodujú
    /// </summary>
    public class StringEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Vts.Gui.Wpf.Test.View.TestHelpers;

internal static class TestConverters
{
    internal class TestResourceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => parameter?.ToString() ?? string.Empty;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    internal class TestBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (value is Visibility v) && v == Visibility.Visible;
    }

    internal class TestDoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? string.Empty : System.Convert.ToString(value, culture);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string s && double.TryParse(s, out var d) ? d : Binding.DoNothing;
        }
    }
}
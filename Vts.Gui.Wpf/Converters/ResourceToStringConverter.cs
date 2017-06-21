using System;
using System.Globalization;
using System.Windows.Data;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Converters
{
    /// Converts a boolean to visibility value.
    public class ResourceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return StringLookup.GetLocalizedString((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
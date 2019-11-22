using System;
using System.Globalization;
using System.Windows.Data;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        // Summary:
        //     Modifies the source data before passing it to the target for display in the
        //     UI.
        //
        // Parameters:
        //   value:
        //     The source data being passed to the target.
        //
        //   targetType:
        //     The System.Type of data expected by the target dependency property.
        //
        //   parameter:
        //     An optional parameter to be used in the converter logic.
        //
        //   culture:
        //     The culture of the conversion.
        //
        // Returns:
        //     The value to be passed to the target dependency property.

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d1;
            if (!(value is double || value is int))
                throw new ArgumentException(StringLookup.GetLocalizedString("Exception_DoubleOrInt"));

            if (value is int)
                d1 = (int)value;
            else
                d1 = (double)value;

            if (parameter != null)
            {
                return d1.ToString((string) parameter, CultureInfo.CurrentCulture);
            }
            return d1.ToString(CultureInfo.CurrentCulture);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string))
                throw new ArgumentException(StringLookup.GetLocalizedString("Exception_String"));
            double d;            
            if (double.TryParse((string) value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out d))            
                return d;
            else
                return 0;
        }

        #endregion
    }
}
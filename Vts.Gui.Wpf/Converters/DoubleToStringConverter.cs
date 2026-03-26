using System;
using System.Globalization;
using System.Windows.Data;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Converters;

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

        if (value is int i)
            d1 = i;
        else
            d1 = (double)value;

        return parameter != null ? d1.ToString((string) parameter, CultureInfo.CurrentCulture) : d1.ToString(CultureInfo.CurrentCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string s)
        {
            throw new ArgumentException(StringLookup.GetLocalizedString("Exception_String"));
        }

        if (double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out var d)) return d;
        return 0;
    }

    #endregion
}
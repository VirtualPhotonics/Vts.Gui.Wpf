using System.Windows;
using NUnit.Framework;
using Vts.Gui.Wpf.Converters;

namespace Vts.Gui.Wpf.Test.Converters
{
    /// <summary>
    /// Tests BooleanToVisibilityConverter classe
    /// </summary>
    [TestFixture]
    public class BooleanToVisibilityConverterTests
    {
        /// <summary>
        /// Verifies method Convert returns correct value
        /// </summary>
        [Test]
        public void verify_method_Convert_returns_correct_value()
        {
            var btvConverter = new BooleanToVisibilityConverter();
            Assert.That(btvConverter.Convert(
                true,  // boolean to convert
                typeof(Visibility),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(Visibility.Visible));
            Assert.That(btvConverter.Convert(
                false, // boolean to convert
                typeof(Visibility),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(Visibility.Collapsed));
        }
        /// <summary>
        /// Verifies method ConvertBack returns correct value
        /// </summary>
        [Test]
        public void verify_method_ConvertBack_returns_correct_value()
        {
            var btvConverter = new BooleanToVisibilityConverter();
            Assert.That(btvConverter.ConvertBack(
                Visibility.Visible,  // Visibility to convert
                typeof(Visibility),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(true));
            Assert.That(btvConverter.ConvertBack(
                Visibility.Collapsed, // Visibility to convert
                typeof(Visibility),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(false));
        }
    }
}

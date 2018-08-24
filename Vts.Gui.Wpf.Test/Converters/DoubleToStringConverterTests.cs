using System;
using System.Windows.Media.Animation;
using NLog.LayoutRenderers.Wrappers;
using NUnit.Framework;
using Vts.Gui.Wpf.Converters;

namespace Vts.Gui.Wpf.Test.Converters
{
    /// <summary>
    /// Tests DoubleToString classe
    /// </summary>
    [TestFixture]
    public class DoubleToStringConverterTests
    {
        private static readonly double d1 = 12.3;
        private readonly double d2 = 12;
        private static readonly int i1 = 12;
        private readonly string s1 = d1.ToString(System.Globalization.CultureInfo.CurrentCulture);
        private readonly string s2 = i1.ToString(System.Globalization.CultureInfo.CurrentCulture);


        /// <summary>
        /// Verifies method Convert returns correct value
        /// </summary>
        [Test]
        public void verify_method_Convert_returns_correct_value()
        {
            var dtsConverter = new DoubleToStringConverter();
            Assert.That(dtsConverter.Convert(
                12.3,  // double to convert
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(d1.ToString(System.Globalization.CultureInfo.CurrentCulture)));
            Assert.That(dtsConverter.Convert(
                12,  // int to convert - OK
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(d2.ToString(System.Globalization.CultureInfo.CurrentCulture)));
            var exception = Assert.Throws<ArgumentException>(() => dtsConverter.Convert(
                "string", // string not a double
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture));
            Assert.That(exception.Message, Is.EqualTo("Value must be a double or int"));
        }
        /// <summary>
        /// Verifies method ConvertBack returns correct value
        /// </summary>
        [Test]
        public void verify_method_ConvertBack_returns_correct_value()
        {
            var dtsConverter = new DoubleToStringConverter();
            Assert.That(dtsConverter.ConvertBack(
                s1,  // string to convert (double)
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(d1));
            Assert.That(dtsConverter.ConvertBack(
                s2, // string to convert (int)
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals((double)i1));
            Assert.That(dtsConverter.ConvertBack(
                "t", // string to convert (NaN)
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(0));
            var exception = Assert.Throws<ArgumentException>(() => dtsConverter.ConvertBack(
                12, // int value
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture));
            Assert.That(exception.Message, Is.EqualTo("Value must be a string"));
        }

    }
}

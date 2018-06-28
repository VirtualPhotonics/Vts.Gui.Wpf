using System;
using NUnit.Framework;
using Vts.Gui.Wpf.Converters;

namespace Vts.Gui.Wpf.Test.Converters
{
    /// <summary>
    /// Tests DoubleToString classe
    /// </summary>
    [TestFixture]
    public class DoubleToStringTests
    {
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
                System.Globalization.CultureInfo.CurrentCulture).Equals("12.3"));
            var exception = Assert.Throws<ArgumentException>(() => dtsConverter.Convert(
                12, // double with no period to convert
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture));
            Assert.That(exception.Message, Is.EqualTo("Value must be a double"));
        }
        /// <summary>
        /// Verifies method ConvertBack returns correct value
        /// </summary>
        [Test]
        public void verify_method_ConvertBack_returns_correct_value()
        {
            var dtsConverter = new DoubleToStringConverter();
            Assert.That(dtsConverter.ConvertBack(
                "12.3",  // string to convert
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(12.3));
            Assert.That(dtsConverter.ConvertBack(
                "12", // string to convert
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture).Equals(12.0));
            Assert.That(dtsConverter.ConvertBack(
                "t", // string to convert
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

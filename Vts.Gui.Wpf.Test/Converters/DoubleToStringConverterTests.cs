using NUnit.Framework;
using System;
using System.Threading;
using Vts.Gui.Wpf.Converters;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Test.Converters
{
    /// <summary>
    /// Tests DoubleToString classes
    /// </summary>
    [TestFixture]
    public class DoubleToStringConverterTests
    {
        private static readonly double D1 = 12.3;
        private readonly double _d2 = 12;
        private static readonly int I1 = 12;
        private readonly string _s1 = D1.ToString(System.Globalization.CultureInfo.CurrentCulture);
        private readonly string _s2 = I1.ToString(System.Globalization.CultureInfo.CurrentCulture);


        /// <summary>
        /// Verifies method Convert returns correct value
        /// </summary>
        [Test]
        public void Verify_method_Convert_returns_correct_value()
        {
            var dtsConverter = new DoubleToStringConverter();
            Assert.That(dtsConverter.Convert(
                D1,  // double to convert
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture), Is.EqualTo(D1.ToString(System.Globalization.CultureInfo.CurrentCulture)));
            Assert.That(dtsConverter.Convert(
                I1,  // int to convert - OK
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture), Is.EqualTo(_d2.ToString(System.Globalization.CultureInfo.CurrentCulture)));
            var exception = Assert.Throws<ArgumentException>(() => dtsConverter.Convert(
                "string", // string not a double
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture));
            Assert.That(exception?.Message, Is.EqualTo(StringLookup.GetLocalizedString("Exception_DoubleOrInt")));
            Assert.That(dtsConverter.Convert(
                1.5,  // double to convert
                typeof(string),
                "N2", // pass a formatting parameter
                System.Globalization.CultureInfo.CurrentCulture), Is.EqualTo($"{1.50.ToString("0.00", Thread.CurrentThread.CurrentCulture)}"));
        }

        /// <summary>
        /// Verifies method ConvertBack returns correct value
        /// </summary>
        [Test]
        public void Verify_method_ConvertBack_returns_correct_value()
        {
            var dtsConverter = new DoubleToStringConverter();
            Assert.That(dtsConverter.ConvertBack(
                _s1,  // string to convert (double)
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture), Is.EqualTo(D1));
            Assert.That(dtsConverter.ConvertBack(
                _s2, // string to convert (int)
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture), Is.EqualTo((double)I1));
            Assert.That(dtsConverter.ConvertBack(
                "t", // string to convert (NaN)
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture), Is.EqualTo(0));
            var exception = Assert.Throws<ArgumentException>(() => dtsConverter.ConvertBack(
                12, // int value
                typeof(string),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture));
            Assert.That(exception?.Message, Is.EqualTo(StringLookup.GetLocalizedString("Exception_String")));
        }

    }
}

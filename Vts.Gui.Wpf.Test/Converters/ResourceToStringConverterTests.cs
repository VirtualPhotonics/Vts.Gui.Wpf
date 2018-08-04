using System;
using NUnit.Framework;
using Vts.Gui.Wpf.Converters;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Test.Converters
{
    /// <summary>
    /// Tests ResourceToStringConverter classe
    /// </summary>
    [TestFixture]
    public class ResourceToStringConverterTests
    {
        /// <summary>
        /// Verifies method Convert returns correct value
        /// </summary>
        [Test]
        public void verify_method_Convert_returns_correct_value()
        {
            var rtsConverter = new ResourceToStringConverter();
            Assert.That(rtsConverter.Convert(
                null,  
                typeof(String),
                "Label_FwdSolver", // resource is passed as a prameter
                System.Globalization.CultureInfo.CurrentCulture).Equals("Fwd Solver:"));
        }
        /// <summary>
        /// Verifies method ConvertBack returns correct value
        /// </summary>
        [Test]
        public void verify_method_ConvertBack_returns_correct_value()
        {
            var rtsConverter = new ResourceToStringConverter();
            var exception = Assert.Throws<NotSupportedException>(() => rtsConverter.ConvertBack(
                "Fwd Solver:", //
                typeof(String),
                null, // no parameters
                System.Globalization.CultureInfo.CurrentCulture));
            Assert.That(exception.Message, Is.EqualTo("Specified method is not supported."));
        }

    }
}

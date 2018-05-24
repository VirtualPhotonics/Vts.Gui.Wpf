using System;
using System.Numerics;
using NUnit.Framework;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    /// <summary>
    /// DataPointTests tests ComplexDataPoint and DataPoint classes
    /// </summary>
    [TestFixture]
    public class DataPointTests
    {
        /// <summary>
        /// Verifies ComplexDataPoint class sets correct values
        /// </summary>
        [Test]
        public void verify_ComplexDataPoint_sets_correct_x_and_y_values()
        {
            var dataPoint = new ComplexDataPoint(0.1, new Complex(0.2, 0.3));
            Assert.AreEqual(dataPoint.X, 0.1);
            Assert.AreEqual(dataPoint.Y.Real, 0.2);
            Assert.AreEqual(dataPoint.Y.Imaginary, 0.3);
        }

        /// <summary>
        /// Verifies ComplexDataPoint symbols !=, == methods Equals work correctly
        /// </summary>
        [Test]
        public void verify_ComplexDataPoint_equals_and_notequal_symbols_and_methods_work_correctly()
        {
            var dataPoint1 = new ComplexDataPoint(0.1, new Complex(0.2, 0.3));
            var dataPoint2 = new ComplexDataPoint(0.1, new Complex(0.2, 0.4));
            Assert.AreEqual(dataPoint1 != dataPoint2, true);
            Assert.AreEqual(dataPoint1 == dataPoint2, false);
            Assert.IsFalse(dataPoint1.Equals(dataPoint2));
            var dataPoint3 = new ComplexDataPoint(0.1, new Complex(0.2, 0.3));
            Assert.AreEqual(dataPoint1 != dataPoint3, false);
            Assert.AreEqual(dataPoint1 == dataPoint3, true);
            Assert.IsTrue(dataPoint1.Equals(dataPoint3));
        }

        /// <summary>
        /// Verifies DoubleDataPoint class sets correct values
        /// </summary>
        [Test]
        public void verify_DoubleDataPoint_sets_correct_x_and_y_values()
        {
            var dataPoint = new DoubleDataPoint(0.1, 0.2);
            Assert.AreEqual(dataPoint.X, 0.1);
            Assert.AreEqual(dataPoint.Y, 0.2);
        }

        /// <summary>
        /// Verifies DataPoint symbols !=, == methods Equals work correctly
        /// </summary>
        [Test]
        public void verify_DataPoint_equals_and_notequal_symbols_and_methods_work_correctly()
        {
            var dataPoint1 = new DoubleDataPoint(0.1, 0.2);
            var dataPoint2 = new DoubleDataPoint(0.1, 0.3);
            Assert.AreEqual(dataPoint1 != dataPoint2, true);
            Assert.AreEqual(dataPoint1 == dataPoint2, false);
            Assert.IsFalse(dataPoint1.Equals(dataPoint2));
            var dataPoint3 = new DoubleDataPoint(0.1, 0.2);
            Assert.AreEqual(dataPoint1 != dataPoint3, false);
            Assert.AreEqual(dataPoint1 == dataPoint3, true);
            Assert.IsTrue(dataPoint1.Equals(dataPoint3));
        }

    }
}

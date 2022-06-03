using System.Numerics;
using System.Threading;
using NUnit.Framework;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    internal class ComplexDataPointTests
    {
        /// <summary>
        /// Verifies ComplexDataPoint class sets correct values
        /// </summary>
        [Test]
        public void Verify_ComplexDataPoint_sets_correct_x_and_y_values()
        {
            var dataPoint = new ComplexDataPoint(0.1, new Complex(0.2, 0.3));
            Assert.AreEqual(0.1, dataPoint.X);
            Assert.AreEqual(0.2,dataPoint.Y.Real);
            Assert.AreEqual(0.3, dataPoint.Y.Imaginary);
        }

        /// <summary>
        /// Verifies ComplexDataPoint Equals method works correctly
        /// </summary>
        [Test]
        public void Verify_ComplexDataPoint_equals_methods_work_correctly()
        {
            var dataPoint1 = new ComplexDataPoint(0.1, new Complex(0.2, 0.3));
            var dataPoint2 = new ComplexDataPoint(0.1, new Complex(0.2, 0.4));
            Assert.AreEqual(false, dataPoint1.Equals(dataPoint2));
            Assert.IsFalse(dataPoint1.Equals(dataPoint2));
            var dataPoint3 = new ComplexDataPoint(0.1, new Complex(0.2, 0.3));
            Assert.AreEqual(true, dataPoint1.Equals(dataPoint3));
            Assert.IsTrue(dataPoint1.Equals(dataPoint3));
            Assert.IsTrue(dataPoint1.Equals(dataPoint1, dataPoint3));
        }

        [Test]
        public void Verify_to_string_value()
        {
            var dataPoint = new ComplexDataPoint(0.1, new Complex(0.3, 0.1));
            var localizedString = $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)}, (" +
                                  $"{0.3.ToString(Thread.CurrentThread.CurrentCulture)}, " +
                                  $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)})";
            Assert.AreEqual(localizedString, dataPoint.ToString());
        }

        [Test]
        public void Verify_hash_code()
        {
            var dataPoint = new ComplexDataPoint(0.8, new Complex(0.2, 0.5));
            var hashCode = dataPoint.GetHashCode();
            Assert.AreEqual(-1892473190, hashCode);
        }

        [Test]
        public void Verify_hash_code_with_parameter()
        {
            var dataPoint1 = new ComplexDataPoint(0.8, new Complex(0.2, 0.5));
            var dataPoint2 = new ComplexDataPoint(0.8, new Complex(0.9, 0.5));
            var hashCode = dataPoint1.GetHashCode(dataPoint2);
            Assert.AreEqual(-1882801729, hashCode);
        }

        [Test]
        public void Verify_equals()
        {
            object dataPoint1 = new ComplexDataPoint(0.8, new Complex(0.1, 0.2));
            object dataPoint2 = new ComplexDataPoint(0.8, new Complex(0.1, 0.2));
            Assert.IsTrue(dataPoint1.Equals(dataPoint2));
        }
    }
}

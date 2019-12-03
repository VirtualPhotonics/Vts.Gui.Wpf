using NUnit.Framework;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    internal class DoubleDataPointTests
    {
        /// <summary>
        /// Verifies DoubleDataPoint class sets correct values
        /// </summary>
        [Test]
        public void Verify_DoubleDataPoint_sets_correct_x_and_y_values()
        {
            var dataPoint = new DoubleDataPoint(0.1, 0.2);
            Assert.AreEqual(0.1, dataPoint.X);
            Assert.AreEqual(0.2,dataPoint.Y);
        }

        /// <summary>
        /// Verifies DataPoint Equals method works correctly
        /// </summary>
        [Test]
        public void Verify_DataPoint_equal_methods_work_correctly()
        {
            var dataPoint1 = new DoubleDataPoint(0.1, 0.2);
            var dataPoint2 = new DoubleDataPoint(0.1, 0.3);
            Assert.AreEqual(false, dataPoint1.Equals(dataPoint2));
            Assert.IsFalse(dataPoint1.Equals(dataPoint2));
            var dataPoint3 = new DoubleDataPoint(0.1, 0.2);
            Assert.AreEqual(true, dataPoint1.Equals(dataPoint3));
            Assert.IsTrue(dataPoint1.Equals(dataPoint3));
            Assert.IsTrue(dataPoint1.Equals(dataPoint1, dataPoint3));
        }

        [Test]
        public void Verify_to_string_value()
        {
            var dataPoint = new DoubleDataPoint(0.1, 0.3);
            Assert.AreEqual("0.1, 0.3", dataPoint.ToString());
        }

        [Test]
        public void Verify_hash_code()
        {
            var dataPoint = new DoubleDataPoint(0.8, 0.2);
            var hashCode = dataPoint.GetHashCode();
            Assert.AreEqual(562847809, hashCode);
        }

        [Test]
        public void Verify_hash_code_with_parameter()
        {
            var dataPoint1 = new DoubleDataPoint(0.8, 0.2);
            var dataPoint2 = new DoubleDataPoint(0.6, 0.12);
            var hashCode = dataPoint1.GetHashCode(dataPoint2);
            Assert.AreEqual(-1439151695, hashCode);
        }
    }
}

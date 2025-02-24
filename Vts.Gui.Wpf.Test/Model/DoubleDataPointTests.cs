using System.Globalization;
using System.Threading;
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
            Assert.That(dataPoint.X, Is.EqualTo(0.1));
            Assert.That(dataPoint.Y, Is.EqualTo(0.2));
        }

        /// <summary>
        /// Verifies DataPoint Equals method works correctly
        /// </summary>
        [Test]
        public void Verify_DataPoint_equal_methods_work_correctly()
        {
            var dataPoint1 = new DoubleDataPoint(0.1, 0.2);
            var dataPoint2 = new DoubleDataPoint(0.1, 0.3);
            Assert.That(dataPoint1.Equals(dataPoint2), Is.EqualTo(false));
            Assert.That(dataPoint1.Equals(dataPoint2), Is.False);
            var dataPoint3 = new DoubleDataPoint(0.1, 0.2);
            Assert.That(dataPoint1.Equals(dataPoint3), Is.EqualTo(true));
            Assert.That(dataPoint1.Equals(dataPoint3), Is.True);
            Assert.That(dataPoint1.Equals(dataPoint1, dataPoint3), Is.True);
        }

        [Test]
        public void Verify_to_string_value()
        {
            var dataPoint = new DoubleDataPoint(0.1, 0.3);
            var localizedString = $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)}, {0.3.ToString(Thread.CurrentThread.CurrentCulture)}";
            Assert.That(dataPoint.ToString(), Is.EqualTo(localizedString));
        }

        [Test]
        public void Verify_hash_code()
        {
            var dataPoint = new DoubleDataPoint(0.8, 0.2);
            var hashCode = dataPoint.GetHashCode();
            Assert.That(hashCode, Is.EqualTo(562847809));
        }

        [Test]
        public void Verify_hash_code_with_parameter()
        {
            var dataPoint1 = new DoubleDataPoint(0.8, 0.2);
            var dataPoint2 = new DoubleDataPoint(0.6, 0.12);
            var hashCode = dataPoint1.GetHashCode(dataPoint2);
            Assert.That(hashCode, Is.EqualTo(-1439151695));
        }

        [Test]
        public void Verify_equals()
        {
            object dataPoint1 = new DoubleDataPoint(0.8, 0.2);
            object dataPoint2 = new DoubleDataPoint(0.8, 0.2);
            Assert.That(dataPoint1.Equals(dataPoint2), Is.True);
        }
    }
}

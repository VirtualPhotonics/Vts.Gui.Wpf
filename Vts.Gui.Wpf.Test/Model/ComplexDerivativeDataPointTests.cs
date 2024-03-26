using System;
using System.Numerics;
using System.Threading;
using NUnit.Framework;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    internal class ComplexDerivativeDataPointTests
    {
        /// <summary>
        /// Verifies ComplexDerivativeDataPoint class sets correct values
        /// </summary>
        [Test]
        public void Verify_ComplexDerivativeDataPoint_sets_correct_x_and_y_values()
        {
            var dataPoint = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.2, 0.3),
                new Complex(0.4, 0.5),
                ForwardAnalysisType.dRdG);
            Assert.AreEqual(0.1, dataPoint.X);
            Assert.AreEqual(0.2,dataPoint.Y.Real);
            Assert.AreEqual(0.3, dataPoint.Y.Imaginary);
            Assert.AreEqual(0.4, dataPoint.Dy.Real);
            Assert.AreEqual(0.5, dataPoint.Dy.Imaginary);
            Assert.AreEqual(ForwardAnalysisType.dRdG, dataPoint.DerivativeVariable);

        }

        /// <summary>
        /// Verifies ComplexDerivativeDataPoint Equals method works correctly
        /// </summary>
        [Test]
        public void Verify_ComplexDerivativeDataPoint_equals_methods_work_correctly()
        {
            var dataPoint1 = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.2, 0.3),
                new Complex(0.4, 0.5),
                ForwardAnalysisType.dRdN);
            var dataPoint2 = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.2, 0.4),
                new Complex(0.4, 0.5),
                ForwardAnalysisType.dRdN);
            Assert.AreEqual(false, dataPoint1.Equals(dataPoint2));
            Assert.IsFalse(dataPoint1.Equals(dataPoint2));
            var dataPoint3 = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.2, 0.3),
            new Complex(0.4, 0.5),
            ForwardAnalysisType.dRdN);
            Assert.AreEqual(true, dataPoint1.Equals(dataPoint3));
            Assert.IsTrue(dataPoint1.Equals(dataPoint3));
            Assert.IsTrue(dataPoint1.Equals(dataPoint1, dataPoint3));
        }

        [Test]
        public void Verify_to_string_value()
        {
            var dataPoint = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.3, 0.1),
                new Complex(0.4, 0.5),
                ForwardAnalysisType.dRdMua);
            var localizedString = $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)}, (" +
                                  $"{0.3.ToString(Thread.CurrentThread.CurrentCulture)}, " +
                                  $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)})";
            Assert.AreEqual(localizedString, dataPoint.ToString());
        }

        [Test]
        public void Verify_hash_code()
        {
            var dataPoint = new ComplexDerivativeDataPoint(
                0.8, 
                new Complex(0.2, 0.5),
                new Complex(0.3, 0.4),
                ForwardAnalysisType.dRdMusp);
            var hashCode = dataPoint.GetHashCode();
            Assert.AreEqual(-1563642927, hashCode);
        }

        [Test]
        public void Verify_hash_code_with_parameter()
        {
            var dataPoint1 = new ComplexDerivativeDataPoint(
                0.8, 
                new Complex(0.2, 0.5),
                new Complex(0.3, 0.6),
                ForwardAnalysisType.dRdG);
            var dataPoint2 = new ComplexDerivativeDataPoint(
                0.8, 
                new Complex(0.9, 0.5),
                new Complex(0.8, 0.7),
                ForwardAnalysisType.dRdMusp);
            var hashCode = dataPoint1.GetHashCode(dataPoint2);
            Assert.AreEqual(hashCode = 867508351, hashCode);
        }

        [Test]
        public void Verify_equals()
        {
            object dataPoint1 = new ComplexDerivativeDataPoint(
                0.8, 
                new Complex(0.1, 0.2),
                new Complex(0.3, 0.4),
                ForwardAnalysisType.R);
            object dataPoint2 = new ComplexDerivativeDataPoint(
                0.8, 
                new Complex(0.1, 0.2),
                new Complex(0.3, 0.4),
                ForwardAnalysisType.R);
            Assert.IsTrue(dataPoint1.Equals(dataPoint2));
        }

        [Test]
        public void Verify_PhaseDerivative()
        {
            object dataPoint = new ComplexDerivativeDataPoint(
                0.8,
                new Complex(0.1, 0.2),
                new Complex(0.3, 0.4),
                ForwardAnalysisType.dRdMua);
            var phaseDerivative = ((ComplexDerivativeDataPoint)dataPoint).PhaseDerivative;
            Assert.IsTrue(Math.Abs(-0.399999 - phaseDerivative) < 1e-6);
        }

        [Test]
        public void Verify_AmplitudeDerivative()
        {
            object dataPoint = new ComplexDerivativeDataPoint(
                0.8,
                new Complex(0.1, 0.2),
                new Complex(0.3, 0.4),
                ForwardAnalysisType.dRdMua);
            var phaseDerivative = ((ComplexDerivativeDataPoint)dataPoint).AmplitudeDerivative;
            Assert.IsTrue(Math.Abs(0.491934 - phaseDerivative) < 1e-6);
        }
    }
}

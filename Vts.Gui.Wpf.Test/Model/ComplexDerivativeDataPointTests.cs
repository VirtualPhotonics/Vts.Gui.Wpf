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
            Assert.That(dataPoint.X, Is.EqualTo(0.1));
            Assert.That(dataPoint.Y.Real, Is.EqualTo(0.2));
            Assert.That(dataPoint.Y.Imaginary, Is.EqualTo(0.3));
            Assert.That(dataPoint.Dy.Real, Is.EqualTo(0.4));
            Assert.That(dataPoint.Dy.Imaginary, Is.EqualTo(0.5));
            Assert.That(dataPoint.DerivativeVariable, Is.EqualTo(ForwardAnalysisType.dRdG));

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
            Assert.That(dataPoint1.Equals(dataPoint2), Is.False);
            var dataPoint3 = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.2, 0.3),
            new Complex(0.4, 0.5),
            ForwardAnalysisType.dRdN);
            Assert.That(dataPoint1.Equals(dataPoint3), Is.True);
            Assert.That(dataPoint1.Equals(dataPoint1, dataPoint3), Is.True);
        }

        [Test]
        public void Verify_to_string_value()
        {
            var dataPoint = new ComplexDerivativeDataPoint(
                0.1, 
                new Complex(0.3, 0.1),
                new Complex(0.4, 0.5),
                ForwardAnalysisType.dRdMua);
            var localizedString = $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)}, <" +
                                  $"{0.3.ToString(Thread.CurrentThread.CurrentCulture)}; " +
                                  $"{0.1.ToString(Thread.CurrentThread.CurrentCulture)}>";
            Assert.That(dataPoint.ToString(), Is.EqualTo(localizedString));
        }

        [Test]
        public void Verify_hash_code()
        {
            var dataPoint = new ComplexDerivativeDataPoint(
                0.8, 
                new Complex(0.2, 0.5),
                new Complex(0.3, 0.4),
                ForwardAnalysisType.dRdMusp);
            Assert.That(dataPoint.GetHashCode(), Is.InstanceOf<int>());
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
            Assert.That(dataPoint1, Is.Not.Null);
            Assert.That(dataPoint2, Is.Not.Null);
            Assert.That(dataPoint1.GetHashCode(dataPoint2), Is.InstanceOf<int>());
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
            Assert.That(dataPoint1.Equals(dataPoint2), Is.True);
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
            Assert.That(Math.Abs(-0.399999 - phaseDerivative) < 1e-6, Is.True);
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
            Assert.That(Math.Abs(0.491934 - phaseDerivative) < 1e-6, Is.True);
        }
    }
}

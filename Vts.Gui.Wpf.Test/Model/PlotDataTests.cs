using NUnit.Framework;
using System.Numerics;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    /// <summary>
    /// PlotDataTests tests PlotData class
    /// </summary>
    [TestFixture]
    public class PlotDataTests
    {
        /// <summary>
        /// Verifies class sets real properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_sets_real_properties_correctly()
        {
            var dataPoints = new IDataPoint[]
            {
                new DoubleDataPoint(0.1, 0.2), 
                new DoubleDataPoint(0.3, 0.4)
            };
            var plotData = new PlotData(dataPoints, "title");
            Assert.That(plotData.Points[0], Is.EqualTo(dataPoints[0]));
            Assert.That(plotData.Points[1], Is.EqualTo(dataPoints[1]));
            Assert.That(plotData.Title, Is.EqualTo("title"));
        }

        /// <summary>
        /// Verifies class sets complex properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_sets_complex_properties_correctly()
        {
            var dataPoints = new IDataPoint[]
            {
                new ComplexDataPoint(0.1, new Complex(0.2, 0.3)), 
                new ComplexDataPoint(0.4, new Complex(0.5, 0.6))
            };
            var plotData = new PlotData(dataPoints, "title");
            Assert.That(plotData.Points[0], Is.EqualTo(dataPoints[0]));
            Assert.That(plotData.Points[1], Is.EqualTo(dataPoints[1]));
            Assert.That(plotData.Title, Is.EqualTo("title"));
        }

    }
}

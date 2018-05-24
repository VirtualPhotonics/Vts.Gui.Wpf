using System;
using System.Numerics;
using NUnit.Framework;
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
        public void verify_constructor_sets_real_properties_correctly()
        {
            IDataPoint[] dataPoints = new DoubleDataPoint[2] 
                {new DoubleDataPoint(0.1, 0.2), new DoubleDataPoint(0.3, 0.4)};
            var plotData = new PlotData(dataPoints, "title");
            Assert.AreEqual(plotData.Points[0], dataPoints[0]);
            Assert.AreEqual(plotData.Points[1], dataPoints[1]);
            Assert.AreEqual(plotData.Title, "title");
        }

        /// <summary>
        /// Verifies class sets complex properties correctly
        /// </summary>
        [Test]
        public void verify_constructor_sets_complex_properties_correctly()
        {
            IDataPoint[] dataPoints = new ComplexDataPoint[2]
                {new ComplexDataPoint(0.1, new Complex(0.2, 0.3)), new ComplexDataPoint(0.4, new Complex(0.5, 0.6))};
            var plotData = new PlotData(dataPoints, "title");
            Assert.AreEqual(plotData.Points[0], dataPoints[0]);
            Assert.AreEqual(plotData.Points[1], dataPoints[1]);
            Assert.AreEqual(plotData.Title, "title");
        }

    }
}

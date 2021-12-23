using NUnit.Framework;
using System;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Test.Model
{
    /// <summary>
    /// PlotDataTests tests MapData class
    /// </summary>
    [TestFixture]
    public class MapDataTests
    {
        private double[] rawData, xValues, yValues, dxValues, dyValues;

        /// <summary>
        /// setup data
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            rawData = new[] { 0.1, 0.2, 0.3, 0.4 };
            xValues = new[] { 0.5, 0.6 };
            yValues = new[] { 0.7, 0.8 };
            dxValues = new[] { 0.9, 1.0 };
            dyValues = new[] { 1.1, 1.2 };
        }

        /// <summary>
        /// Verifies class sets properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_sets_properties_correctly()
        {

            var mapData = new MapData(rawData, xValues, yValues, dxValues, dyValues);
            Assert.AreEqual(mapData.RawData, rawData); // nunit AreEqual compares arrays item by item
            Assert.AreEqual(xValues, mapData.XValues);
            Assert.AreEqual(yValues, mapData.YValues);
            Assert.AreEqual(dxValues, mapData.DxValues);
            Assert.AreEqual(dyValues, mapData.DyValues);
        }

        /// <summary>
        /// Verifies class methods Min, Max, YExpectationValue work correctly
        /// </summary>
        [Test]
        public void Verify_class_methods_work_correctly()
        {
            var mapData = new MapData(rawData, xValues, yValues, dxValues, dyValues);
            Assert.AreEqual(2, mapData.Width); // length of xvalues
            Assert.AreEqual(2, mapData.Height); // length of yvalues
            Assert.AreEqual(0.1, mapData.Min); // min of rawdata
            Assert.AreEqual(0.4, mapData.Max); // max of rawdata
            Assert.Less(Math.Abs(mapData.YExpectationValue - 0.771593), 0.000001);
        }

        /// <summary>
        /// Verifies class method Create works correctly
        /// </summary>
        [Test]
        public void Verify_Create_method_works_correctly()
        {
            var mapData = MapData.Create(rawData, xValues, yValues, dxValues, dyValues);
            Assert.IsInstanceOf<MapData>(mapData);
        }

    }
}

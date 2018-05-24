using System;
using System.Numerics;
using NUnit.Framework;
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
        public void setup()
        {
            rawData = new double[4] { 0.1, 0.2, 0.3, 0.4 };
            xValues = new double[2] { 0.5, 0.6 };
            yValues = new double[2] { 0.7, 0.8 };
            dxValues = new double[2] { 0.9, 1.0 };
            dyValues = new double[2] { 1.1, 1.2 };
        }

        /// <summary>
        /// Verifies class sets properties correctly
        /// </summary>
        [Test]
        public void verify_constructor_sets_properties_correctly()
        {

            var mapData = new MapData(rawData, xValues, yValues, dxValues, dyValues);
            Assert.AreEqual(mapData.RawData, rawData); // nunit AreEqual compares arrays item by item
            Assert.AreEqual(mapData.XValues, xValues);
            Assert.AreEqual(mapData.YValues, yValues);
            Assert.AreEqual(mapData.DxValues, dxValues);
            Assert.AreEqual(mapData.DyValues, dyValues);
        }

        /// <summary>
        /// Verifies class methods Min, Max, YExpectationValue work correctly
        /// </summary>
        [Test]
        public void verify_class_methods_work_correctly()
        {
            var mapData = new MapData(rawData, xValues, yValues, dxValues, dyValues);
            Assert.AreEqual(mapData.Width, 2); // length of xvalues
            Assert.AreEqual(mapData.Height, 2); // length of yvalues
            Assert.AreEqual(mapData.Min, 0.1); // min of rawdata
            Assert.AreEqual(mapData.Max, 0.4); // max of rawdata
            Assert.Less(Math.Abs(mapData.YExpectationValue - 0.771593), 0.000001);
        }

        /// <summary>
        /// Verifies class method Create works correctly
        /// </summary>
        [Test]
        public void verify_Create_method_works_correctly()
        {
            var mapData = MapData.Create(rawData, xValues, yValues, dxValues, dyValues);
            Assert.IsTrue(mapData != null);
        }

    }
}

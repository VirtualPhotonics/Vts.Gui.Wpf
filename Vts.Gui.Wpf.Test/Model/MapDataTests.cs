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
        private double[] _rawData, _xValues, _yValues, _dxValues, _dyValues;

        /// <summary>
        /// setup data
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            _rawData = new[] { 0.1, 0.2, 0.3, 0.4 };
            _xValues = new[] { 0.5, 0.6 };
            _yValues = new[] { 0.7, 0.8 };
            _dxValues = new[] { 0.9, 1.0 };
            _dyValues = new[] { 1.1, 1.2 };
        }

        /// <summary>
        /// Verifies class sets properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_sets_properties_correctly()
        {

            var mapData = new MapData(_rawData, _xValues, _yValues, _dxValues, _dyValues);
            Assert.AreEqual(mapData.RawData, _rawData); // nunit AreEqual compares arrays item by item
            Assert.AreEqual(_xValues, mapData.XValues);
            Assert.AreEqual(_yValues, mapData.YValues);
            Assert.AreEqual(_dxValues, mapData.DxValues);
            Assert.AreEqual(_dyValues, mapData.DyValues);
        }

        /// <summary>
        /// Verifies class methods Min, Max, YExpectationValue work correctly
        /// </summary>
        [Test]
        public void Verify_class_methods_work_correctly()
        {
            var mapData = new MapData(_rawData, _xValues, _yValues, _dxValues, _dyValues);
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
            var mapData = MapData.Create(_rawData, _xValues, _yValues, _dxValues, _dyValues);
            Assert.IsInstanceOf<MapData>(mapData);
        }

        /// <summary>
        /// Verifies class method Create works correctly
        /// </summary>
        [Test]
        public void Verify_Create_method_throws_exception()
        {
            var xValues = new[] { 0.5, 0.6 };
            var yValues = new[] { 0.7, 0.8, 0.1 };
            Assert.Throws<ArgumentException>(() => MapData.Create(_rawData, xValues, yValues, _dxValues, _dyValues));
        }

        /// <summary>
        /// Verifies class method Create works correctly
        /// </summary>
        [Test]
        public void Verify_Create_method_with_double_array_works_correctly()
        {
            var rawData = new[,] { { 0.1, 0.2 }, { 0.3, 0.4 } };
            var mapData = MapData.Create(rawData, _xValues, _yValues, _dxValues, _dyValues);
            Assert.IsInstanceOf<MapData>(mapData);
        }

        /// <summary>
        /// Verifies class method Create works correctly
        /// </summary>
        [Test]
        public void Verify_Create_method_with_double_array_throws_exception()
        {
            var rawData = new[,] {{ 0.1, 0.2 }, { 0.3, 0.4 }};
            var xValues = new[] { 0.5, 0.6 };
            var yValues = new[] { 0.7, 0.8, 0.1 };
            Assert.Throws<ArgumentException>(() => MapData.Create(rawData, xValues, yValues, _dxValues, _dyValues));
        }
    }
}

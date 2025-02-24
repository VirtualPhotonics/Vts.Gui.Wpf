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
            Assert.That(_rawData, Is.EqualTo(mapData.RawData)); // nunit AreEqual compares arrays item by item
            Assert.That(mapData.XValues, Is.EqualTo(_xValues));
            Assert.That(mapData.YValues, Is.EqualTo(_yValues));
            Assert.That(mapData.DxValues, Is.EqualTo(_dxValues));
            Assert.That(mapData.DyValues, Is.EqualTo(_dyValues));
        }

        /// <summary>
        /// Verifies class methods Min, Max, YExpectationValue work correctly
        /// </summary>
        [Test]
        public void Verify_class_methods_work_correctly()
        {
            var mapData = new MapData(_rawData, _xValues, _yValues, _dxValues, _dyValues);
            Assert.That(mapData.Width, Is.EqualTo(2)); // length of x values
            Assert.That(mapData.Height, Is.EqualTo(2)); // length of y values
            Assert.That(mapData.Min, Is.EqualTo(0.1)); // min of raw data
            Assert.That(mapData.Max, Is.EqualTo(0.4)); // max of raw data
            Assert.That(Math.Abs(mapData.YExpectationValue - 0.771593), Is.LessThan(0.000001));
        }

        /// <summary>
        /// Verifies class method Create works correctly
        /// </summary>
        [Test]
        public void Verify_Create_method_works_correctly()
        {
            var mapData = MapData.Create(_rawData, _xValues, _yValues, _dxValues, _dyValues);
            Assert.That(mapData, Is.InstanceOf<MapData>());
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
            Assert.That(mapData, Is.InstanceOf<MapData>());
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

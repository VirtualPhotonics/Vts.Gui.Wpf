using NUnit.Framework;
using Vts.Common;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls
{
    /// <summary>
    /// Tests for RangeViewModel class
    /// </summary>
    [TestFixture]
    public class RangeViewModelTests
    {
        private RangeViewModel _rangeViewModel;

        [OneTimeSetUp]
        public void Setup()
        {
            _rangeViewModel = new RangeViewModel(
                new DoubleRange(0.0, 100.0, 101),
                "xx",
                IndependentVariableAxis.Time,
                "Test:",
                false);
        }

        /// <summary>
        /// Verifies that RangeViewModel default constructor instantiates properties correctly
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var rangeVM = new RangeViewModel();
            Assert.AreEqual(1.0, rangeVM.Start);
            Assert.AreEqual(6, rangeVM.Stop);
            Assert.AreEqual(60, rangeVM.Number);
        }

        /// <summary>
        /// Verifies that RangeViewModel constructor with arguments works correctly
        /// </summary>
        [Test]
        public void Verify_constructor_with_arguments_sets_properties_correctly()
        {
            var rangeVM = new RangeViewModel(
                new DoubleRange(0.0, 100.0, 101), 
                "xx", 
                IndependentVariableAxis.Time, 
                "Test:", 
                false);
            Assert.AreEqual(0.0, rangeVM.Start);
            Assert.AreEqual(100.0, rangeVM.Stop);
            Assert.AreEqual(101, rangeVM.Number);
            Assert.AreEqual("xx", rangeVM.Units);
            Assert.AreEqual(IndependentVariableAxis.Time, rangeVM.AxisType);
            Assert.AreEqual("Test:", rangeVM.Title);
            Assert.IsFalse(rangeVM.EnableNumber);
        }

        [Test]
        public void Verify_show_title_returns_true()
        {
            Assert.IsTrue(_rangeViewModel.ShowTitle);
        }

        [Test]
        public void Verify_enable_number_returns_true()
        {
            Assert.IsFalse(_rangeViewModel.EnableNumber);
            _rangeViewModel.EnableNumber = true;
            Assert.IsTrue(_rangeViewModel.EnableNumber);
        }

        [Test]
        public void Verify_setting_axis_type_changes_value()
        {
            Assert.AreEqual(IndependentVariableAxis.Time, _rangeViewModel.AxisType);
            _rangeViewModel.AxisType = IndependentVariableAxis.Rho;
            Assert.AreEqual(IndependentVariableAxis.Rho, _rangeViewModel.AxisType);
        }
    }
}

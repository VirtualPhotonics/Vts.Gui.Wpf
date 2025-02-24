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
            Assert.That(rangeVM.Start, Is.EqualTo(1.0));
            Assert.That(rangeVM.Stop, Is.EqualTo(6));
            Assert.That(rangeVM.Number, Is.EqualTo(60));
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
            Assert.That(rangeVM.Start, Is.EqualTo(0.0));
            Assert.That(rangeVM.Stop, Is.EqualTo(100.0));
            Assert.That(rangeVM.Number, Is.EqualTo(101));
            Assert.That(rangeVM.Units, Is.EqualTo("xx"));
            Assert.That(rangeVM.AxisType, Is.EqualTo(IndependentVariableAxis.Time));
            Assert.That(rangeVM.Title, Is.EqualTo("Test:"));
            Assert.That(rangeVM.EnableNumber, Is.False);
        }

        [Test]
        public void Verify_show_title_returns_true()
        {
            Assert.That(_rangeViewModel.ShowTitle, Is.True);
        }

        [Test]
        public void Verify_enable_number_returns_true()
        {
            Assert.That(_rangeViewModel.EnableNumber, Is.False);
            _rangeViewModel.EnableNumber = true;
            Assert.That(_rangeViewModel.EnableNumber, Is.True);
        }

        [Test]
        public void Verify_setting_axis_type_changes_value()
        {
            Assert.That(_rangeViewModel.AxisType, Is.EqualTo(IndependentVariableAxis.Time));
            _rangeViewModel.AxisType = IndependentVariableAxis.Rho;
            Assert.That(_rangeViewModel.AxisType, Is.EqualTo(IndependentVariableAxis.Rho));
        }
    }
}

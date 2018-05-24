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
        /// <summary>
        /// Verifies that RangeViewModel default constructor instantiates properties correctly
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var rangeVM = new RangeViewModel();
            Assert.AreEqual(rangeVM.Start, 1.0);
            Assert.AreEqual(rangeVM.Stop, 6);
            Assert.AreEqual(rangeVM.Number, 60);
        }

        /// <summary>
        /// Verifies that RangeViewModel constructor with arguments works correctly
        /// </summary>
        [Test]
        public void verify_constructor_with_argumenbts_sets_properties_correctly()
        {
            var rangeVM = new RangeViewModel(
                new DoubleRange(0.0, 100.0, 101), 
                "xx", 
                IndependentVariableAxis.Time, 
                "Test:", 
                false);
            Assert.AreEqual(rangeVM.Start, 0.0);
            Assert.AreEqual(rangeVM.Stop, 100.0);
            Assert.AreEqual(rangeVM.Number, 101);
            Assert.AreEqual(rangeVM.Units, "xx");
            Assert.AreEqual(rangeVM.AxisType, IndependentVariableAxis.Time);
            Assert.AreEqual(rangeVM.Title, "Test:");
            Assert.AreEqual(rangeVM.EnableNumber, false);
        }

    }
}

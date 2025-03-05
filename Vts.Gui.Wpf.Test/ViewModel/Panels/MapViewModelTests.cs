using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for MapViewModelTests
    /// </summary>
    [TestFixture]
    public class MapViewModelTests
    {

        /// <summary>
        /// Verifies that MapViewModel default constructor 
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new MapViewModel();
            Assert.That(viewModel.MinValue, Is.EqualTo(1e-9));
            Assert.That(viewModel.MaxValue, Is.EqualTo(1.0));
            Assert.That(viewModel.ScalingTypeOptionVm, Is.InstanceOf<OptionViewModel<ScalingType>>());
            Assert.That(viewModel.ColormapTypeOptionVm, Is.InstanceOf<OptionViewModel<ColormapType>>());
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// Verifies that the ClearMapCommand returns correct values
        /// </summary>
        [Test]
        public void Verify_ClearMapCommand_returns_correct_values()
        {
            var viewModel = new MapViewModel();
            viewModel.PlotMap.Execute(null);
            Assert.That(viewModel.Bitmap, Is.Null);

        }

        /// ExportDataToTextCommand brings up Dialog window so not tested
        /// DuplicateWindowCommand - not sure if can test 
    }
}
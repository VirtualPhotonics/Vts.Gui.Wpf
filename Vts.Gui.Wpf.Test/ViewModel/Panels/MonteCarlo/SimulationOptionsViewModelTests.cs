using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for SimulationOptionsViewModelTests
    /// </summary>
    [TestFixture]
    public class SimulationOptionsViewModelTests
    {
        /// <summary>
        /// Verifies that SimulationOptionsModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new SimulationOptionsViewModel();
            Assert.IsTrue(viewModel.AbsorptionWeightingTypeVM != null);
            Assert.IsTrue(viewModel.PhaseFunctionTypeVM != null);
            Assert.IsTrue(viewModel.RandomNumberGeneratorTypeVM != null);
            Assert.IsTrue(viewModel.RandomNumberGeneratorTypeVM != null);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// SetStatisticsFolderCommand brings up Dialog window so no test 
        /// </summary>
   
    }
}

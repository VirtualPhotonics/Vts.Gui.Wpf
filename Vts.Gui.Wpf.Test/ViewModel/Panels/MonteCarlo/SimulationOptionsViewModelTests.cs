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
            Assert.That(viewModel.AbsorptionWeightingTypeVM != null, Is.True);
            Assert.That(viewModel.PhaseFunctionTypeVM != null, Is.True);
            Assert.That(viewModel.RandomNumberGeneratorTypeVM != null, Is.True);
            Assert.That(viewModel.RandomNumberGeneratorTypeVM != null, Is.True);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// SetStatisticsFolderCommand brings up Dialog window so no test 
        /// </summary>
   
    }
}

using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for SimulationInputViewModelTests
    /// </summary>
    [TestFixture]
    public class SimulationInputViewModelTests
    {
        /// <summary>
        /// Verifies that SimulationInputModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new SimulationInputViewModel();
            Assert.That(viewModel.SimulationOptionsVM != null, Is.True);
            Assert.That(viewModel.TissueInputVM != null, Is.True);
            Assert.That(viewModel.TissueTypeVM != null, Is.True);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// no relay commands in this class 
        /// </summary>
   
    }
}

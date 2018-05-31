using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.SubPanels
{
    /// <summary>
    /// Summary description for SolutionDomainOptionViewModelTests
    /// </summary>
    [TestFixture]
    public class SolutionDomainOptionViewModelTests
    {
        /// <summary>
        /// Verifies that SolutionDomainOptionModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new SolutionDomainOptionViewModel();
            Assert.AreEqual(viewModel.ROfRhoOption, viewModel.Options[SolutionDomainType.ROfRho]);
            Assert.AreEqual(viewModel.ROfFxOption, viewModel.Options[SolutionDomainType.ROfFx]);
            Assert.AreEqual(viewModel.ROfRhoAndTimeOption, viewModel.Options[SolutionDomainType.ROfRhoAndTime]);
            Assert.AreEqual(viewModel.ROfFxAndTimeOption, viewModel.Options[SolutionDomainType.ROfFxAndTime]);
            Assert.AreEqual(viewModel.ROfRhoAndFtOption, viewModel.Options[SolutionDomainType.ROfRhoAndFt]);
            Assert.AreEqual(viewModel.ROfFxAndFtOption, viewModel.Options[SolutionDomainType.ROfFxAndFt]);
        }

        // could add test for UpdateOptions results here
    }
}

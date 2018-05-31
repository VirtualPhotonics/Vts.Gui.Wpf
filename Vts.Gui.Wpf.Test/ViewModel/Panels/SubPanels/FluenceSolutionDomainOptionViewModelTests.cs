using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.SubPanels
{
    /// <summary>
    /// Summary description for FluenceSolutionDomainOptionViewModelTests
    /// </summary>
    [TestFixture]
    public class FluenceSolutionDomainOptionViewModelTests
    {
        /// <summary>
        /// Verifies that FluenceSolutionDomainOptionModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new FluenceSolutionDomainOptionViewModel();
            Assert.AreEqual(viewModel.FluenceOfRhoAndZOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZ]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZ]);
            Assert.AreEqual(viewModel.FluenceOfRhoAndZAndTimeOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZAndTimeOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime]);
            Assert.AreEqual(viewModel.FluenceOfRhoAndZAndFtOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZAndFtOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt]);
        }

        // could add test for results set by UpdateOptions
    }
}

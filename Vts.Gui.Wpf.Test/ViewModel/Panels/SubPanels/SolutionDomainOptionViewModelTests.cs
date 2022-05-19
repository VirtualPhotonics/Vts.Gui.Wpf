using System;
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
        /// Verifies that SolutionDomainOptionModel default constructor instantiates sub ViewModels
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new SolutionDomainOptionViewModel();
            Assert.AreEqual(viewModel.ROfRhoOption, viewModel.Options[SolutionDomainType.ROfRho]);
            Assert.AreEqual(viewModel.ROfFxOption, viewModel.Options[SolutionDomainType.ROfFx]);
            Assert.AreEqual(viewModel.ROfRhoAndTimeOption, viewModel.Options[SolutionDomainType.ROfRhoAndTime]);
            Assert.AreEqual(viewModel.ROfFxAndTimeOption, viewModel.Options[SolutionDomainType.ROfFxAndTime]);
            Assert.AreEqual(viewModel.ROfRhoAndFtOption, viewModel.Options[SolutionDomainType.ROfRhoAndFt]);
            Assert.AreEqual(viewModel.ROfFxAndFtOption, viewModel.Options[SolutionDomainType.ROfFxAndFt]);
        }

        /// <summary>
        /// Test the update options
        /// </summary>
        [Test]
        public void Verify_constructor_sets_update_options_correctly()
        {
            var viewModel = new SolutionDomainOptionViewModel("ROfRhoAndTime", SolutionDomainType.ROfRhoAndTime);
            Assert.AreEqual(SolutionDomainType.ROfRhoAndTime, viewModel.SelectedValue);
            viewModel = new SolutionDomainOptionViewModel("ROfFx", SolutionDomainType.ROfFx);
            Assert.AreEqual(SolutionDomainType.ROfFx, viewModel.SelectedValue);
            viewModel = new SolutionDomainOptionViewModel("ROfFxAndTime", SolutionDomainType.ROfFxAndTime);
            Assert.AreEqual(SolutionDomainType.ROfFxAndTime, viewModel.SelectedValue);
            viewModel = new SolutionDomainOptionViewModel("ROfFxAndFt", SolutionDomainType.ROfFxAndFt);
            Assert.AreEqual(SolutionDomainType.ROfFxAndFt, viewModel.SelectedValue);
            viewModel = new SolutionDomainOptionViewModel("ROfRhoAndFt", SolutionDomainType.ROfRhoAndFt);
            Assert.AreEqual(SolutionDomainType.ROfRhoAndFt, viewModel.SelectedValue);
        }

        [Test]
        public void Verify_update_options_throws_exception()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                var solutionDomainOptionViewModel = new SolutionDomainOptionViewModel("InvalidOption", (SolutionDomainType) 99);
            });
        }
    }
}

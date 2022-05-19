using NUnit.Framework;
using System;
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
        /// Verifies that FluenceSolutionDomainOptionModel default constructor instantiates sub ViewModels
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new FluenceSolutionDomainOptionViewModel();
            Assert.AreEqual(viewModel.FluenceOfRhoAndZOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZ]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZ]);
            Assert.AreEqual(viewModel.FluenceOfRhoAndZAndTimeOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZAndTimeOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime]);
            Assert.AreEqual(viewModel.FluenceOfRhoAndZAndFtOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt]);
            Assert.AreEqual(viewModel.FluenceOfFxAndZAndFtOption, viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt]);
        }

        /// <summary>
        /// Test the update options
        /// </summary>
        [Test]
        public void Verify_constructor_sets_update_options_correctly()
        {
            var viewModel = new FluenceSolutionDomainOptionViewModel();
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfRhoAndZ, viewModel.SelectedValue);
            viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZ].IsSelected = true;
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfFxAndZ, viewModel.SelectedValue);
            viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt].IsSelected = true;
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfFxAndZAndFt, viewModel.SelectedValue);
            viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime].IsSelected = true;
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfFxAndZAndTime, viewModel.SelectedValue);
            viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt].IsSelected = true;
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfRhoAndZAndFt, viewModel.SelectedValue);
            viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime].IsSelected = true;
            Assert.AreEqual(FluenceSolutionDomainType.FluenceOfRhoAndZAndTime, viewModel.SelectedValue);
        }

        [Test]
        public void Verify_update_options_throws_exception()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                var viewModel = new FluenceSolutionDomainOptionViewModel
                {
                    SelectedValue = (FluenceSolutionDomainType) 99
                };
            });
        }
    }
}

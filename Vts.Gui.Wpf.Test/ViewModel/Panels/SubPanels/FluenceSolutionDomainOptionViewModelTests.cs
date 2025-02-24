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
            Assert.That(viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZ], Is.EqualTo(viewModel.FluenceOfRhoAndZOption));
            Assert.That(viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZ], Is.EqualTo(viewModel.FluenceOfFxAndZOption));
            Assert.That(viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime], Is.EqualTo(viewModel.FluenceOfRhoAndZAndTimeOption));
            Assert.That(viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime], Is.EqualTo(viewModel.FluenceOfFxAndZAndTimeOption));
            Assert.That(viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt], Is.EqualTo(viewModel.FluenceOfRhoAndZAndFtOption));
            Assert.That(viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt], Is.EqualTo(viewModel.FluenceOfFxAndZAndFtOption));
        }

        /// <summary>
        /// Test the update options
        /// </summary>
        [Test]
        public void Verify_constructor_sets_update_options_correctly()
        {
            var viewModel = new FluenceSolutionDomainOptionViewModel();
            Assert.That(viewModel.SelectedValue, Is.EqualTo(FluenceSolutionDomainType.FluenceOfRhoAndZ));
            viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZ].IsSelected = true;
            Assert.That(viewModel.SelectedValue, Is.EqualTo(FluenceSolutionDomainType.FluenceOfFxAndZ));
            viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt].IsSelected = true;
            Assert.That(viewModel.SelectedValue, Is.EqualTo(FluenceSolutionDomainType.FluenceOfFxAndZAndFt));
            viewModel.Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime].IsSelected = true;
            Assert.That(viewModel.SelectedValue, Is.EqualTo(FluenceSolutionDomainType.FluenceOfFxAndZAndTime));
            viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt].IsSelected = true;
            Assert.That(viewModel.SelectedValue, Is.EqualTo(FluenceSolutionDomainType.FluenceOfRhoAndZAndFt));
            viewModel.Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime].IsSelected = true;
            Assert.That(viewModel.SelectedValue, Is.EqualTo(FluenceSolutionDomainType.FluenceOfRhoAndZAndTime));
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

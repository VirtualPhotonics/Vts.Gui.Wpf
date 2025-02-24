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
            Assert.That(viewModel.Options[SolutionDomainType.ROfRho], Is.EqualTo(viewModel.ROfRhoOption));
            Assert.That(viewModel.Options[SolutionDomainType.ROfFx], Is.EqualTo(viewModel.ROfFxOption));
            Assert.That(viewModel.Options[SolutionDomainType.ROfRhoAndTime], Is.EqualTo(viewModel.ROfRhoAndTimeOption));
            Assert.That(viewModel.Options[SolutionDomainType.ROfFxAndTime], Is.EqualTo(viewModel.ROfFxAndTimeOption));
            Assert.That(viewModel.Options[SolutionDomainType.ROfRhoAndFt], Is.EqualTo(viewModel.ROfRhoAndFtOption));
            Assert.That(viewModel.Options[SolutionDomainType.ROfFxAndFt], Is.EqualTo(viewModel.ROfFxAndFtOption));
        }

        /// <summary>
        /// Test the update options
        /// </summary>
        [Test]
        public void Verify_constructor_sets_update_options_correctly()
        {
            var viewModel = new SolutionDomainOptionViewModel("ROfRhoAndTime", SolutionDomainType.ROfRhoAndTime);
            Assert.That(viewModel.SelectedValue, Is.EqualTo(SolutionDomainType.ROfRhoAndTime));
            viewModel = new SolutionDomainOptionViewModel("ROfFx", SolutionDomainType.ROfFx);
            Assert.That(viewModel.SelectedValue, Is.EqualTo(SolutionDomainType.ROfFx));
            viewModel = new SolutionDomainOptionViewModel("ROfFxAndTime", SolutionDomainType.ROfFxAndTime);
            Assert.That(viewModel.SelectedValue, Is.EqualTo(SolutionDomainType.ROfFxAndTime));
            viewModel = new SolutionDomainOptionViewModel("ROfFxAndFt", SolutionDomainType.ROfFxAndFt);
            Assert.That(viewModel.SelectedValue, Is.EqualTo(SolutionDomainType.ROfFxAndFt));
            viewModel = new SolutionDomainOptionViewModel("ROfRhoAndFt", SolutionDomainType.ROfRhoAndFt);
            Assert.That(viewModel.SelectedValue, Is.EqualTo(SolutionDomainType.ROfRhoAndFt));
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

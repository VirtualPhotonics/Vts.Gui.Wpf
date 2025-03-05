using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.SubPanels
{
    /// <summary>
    /// Summary description for AbstractSolutionDomainOptionViewModelTests
    /// </summary>
    [TestFixture]
    public class AbstractSolutionDomainOptionViewModelTests
    {
        /// <summary>
        /// Verifies that AbstractSolutionDomainOptionViewModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new AbstractSolutionDomainOptionViewModel<IndependentVariableAxis>();
            Assert.That(viewModel.UseSpectralInputs, Is.False);
            Assert.That(viewModel.AllowMultiAxis, Is.False);
            Assert.That(viewModel.ShowIndependentAxisChoice, Is.False);
        }

        // could add test for UpdateAxes method here
    }
}

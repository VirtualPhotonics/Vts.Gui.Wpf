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
        public AbstractSolutionDomainOptionViewModelTests()
        {
            // constructor logic if needed goes here
        }

        /// <summary>
        /// setup and tear down
        /// </summary>
        [OneTimeSetUp]
        public void setup()
        {
            //clear_folders_and_files();

        }

        [OneTimeTearDown]
        public void clear_folders_and_files()
        {
            //foreach (var folder in listOfInfileFolders)
            //{
            //    if (Directory.Exists(folder))
            //    {
            //        Directory.Delete(folder);
            //    }
            //}

        }

        /// <summary>
        /// Verifies that AbstractSolutionDomainOptionViewModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new AbstractSolutionDomainOptionViewModel<IndependentVariableAxis>();
            Assert.IsFalse(viewModel.UseSpectralInputs);
            Assert.IsFalse(viewModel.AllowMultiAxis);
            Assert.IsFalse(viewModel.ShowIndependentAxisChoice);
        }

        // could add test for UpdateAxes method here
    }
}

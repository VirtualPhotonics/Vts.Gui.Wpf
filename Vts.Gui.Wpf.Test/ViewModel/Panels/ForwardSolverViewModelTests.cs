using NUnit.Framework;
using System.IO;
using Vts.Common;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.ViewModel;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for ForwardSolverViewModelTests
    /// </summary>
    [TestFixture]
    public class ForwardSolverViewModelTests
    {
        public ForwardSolverViewModelTests()
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
        /// Verifies that ForwardSolverViewModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.ForwardSolverVM;
            Assert.IsTrue(viewModel.ForwardSolverTypeOptionVM != null);
            Assert.IsTrue(viewModel.SolutionDomainTypeOptionVM != null);
            Assert.IsTrue(viewModel.ForwardAnalysisTypeOptionVM != null);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// Verifies that the ExecuteForwardSolverCommand returns correct values
        /// </summary>
        //[Test]
        //public void verify_ExecuteForwardSolverCommand_returns_correct_values()
        //{
        //    // WindowViewModel needs to be instantiated for default constructor
        //    var windowViewModel = new WindowViewModel();
        //    var viewModel = windowViewModel.ForwardSolverVM;
        //    viewModel.SolutionDomainTypeOptionVM.SelectedValue = SolutionDomainType.ROfRho;
        //    viewModel.ExecuteForwardSolverCommand.Execute(null);
        //    PlotViewModel plotViewModel = windowViewModel.PlotVM;
        //    //Assert.AreEqual(plotViewModel.PlotValues.

        //}
    }
}

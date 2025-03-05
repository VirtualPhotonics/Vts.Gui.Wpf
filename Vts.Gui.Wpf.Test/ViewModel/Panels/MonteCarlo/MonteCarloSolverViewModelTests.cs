using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for MonteCarloSolverViewModelTests
    /// </summary>
    [TestFixture]
    public class MonteCarloSolverViewModelTests
    {
        readonly List<string> listOfInfileFolders = new List<string>()
        {
            "ellip_FluenceOfRhoAndZ",
            "embeddedDirectionalCircularSourceEllipTissue",
            "Flat_source_one_layer_ROfRho",
            "Gaussian_source_one_layer_ROfRho",
            "one_layer_all_detectors",
            "one_layer_FluenceOfRhoAndZ_RadianceOfRhoAndZAndAngle",
            "one_layer_ROfRho_FluenceOfRhoAndZ",
            "pMC_one_layer_ROfRho_DAW",
            "three_layer_ReflectedTimeOfRhoAndSubregionHist",
            "two_layer_momentum_transfer_detectors",
            "two_layer_ROfRho",
            "two_layer_ROfRho_with_db",
            "voxel_ROfXAndY_FluenceOfXAndYAndZ",
        };

        public MonteCarloSolverViewModelTests()
        {
            // constructor logic if needed goes here
        }

        /// <summary>
        /// setup and tear down
        /// </summary>
        [OneTimeSetUp]
        public void setup()
        {
            clear_folders_and_files();

        }

        [OneTimeTearDown]
        public void clear_folders_and_files()
        {
            foreach (var folder in listOfInfileFolders)
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder);
                }
            }

        }

        /// <summary>
        /// Verifies that MonteCarloSolverViewModel default constructor sets properties correctly
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            MonteCarloSolverViewModel viewModel = new MonteCarloSolverViewModel();
            Assert.That(viewModel.CanDownloadInfiles, Is.True);
            Assert.That(viewModel.CanLoadInputFile, Is.True);
            Assert.That(viewModel.CanRunSimulation, Is.True);
            Assert.That(viewModel.CanRunSimulation, Is.True);
            Assert.That(viewModel.CanCancelSimulation, Is.False);
            Assert.That(viewModel.CanSaveResults, Is.False);
            Assert.That(StringLookup.GetLocalizedString("Button_Cancel"), Is.EqualTo(viewModel.CancelButtonText));
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// LoadSimulationInputCommand brings up Dialog window so not tested
        /// DownloadDefaultSimulationInputCommand brings up Dialog window so not tested
        /// SaveSimulationResultsCommand brings up Dialog window so not tested
        /// </summary>

        /// <summary>
        /// Execute Monte Carlo Solver command that fails and verify properties are set correctly
        /// </summary>
        [Test]
        public void validate_ExecuteMonteCarloSolverCommand_failure_sets_properties_correctly()
        {
            var viewModel = new MonteCarloSolverViewModel();
            // this execution of ExecuteMonteCarloSolverCommand errors in "try"
            // because threading not established, so values are as initialized
            viewModel.ExecuteMonteCarloSolverCommand.Execute(null);
            Assert.That(viewModel.CanRunSimulation, Is.False);
            Assert.That(viewModel.CanLoadInputFile, Is.False);
            Assert.That(viewModel.CanCancelSimulation, Is.True);
            Assert.That(viewModel.CanSaveResults, Is.False);
            Assert.That(viewModel.CancelButtonText == StringLookup.GetLocalizedString("Button_CancelSimulation"), Is.True);
        }
        ///// <summary>
        ///// Execute Monte Carlo Solver command that runs and verify properties are set correctly
        ///// This commented out because threading setup not working yet
        ///// </summary>
        //[Test]
        //public void validate_ExecuteMonteCarloSolverCommand_executes_successfully()
        //{
        //    MainWindow mainWindow = null;
        //    // need to instantiate MainWindow which is needed in main "try" - found following code on stack overflow
        //    Thread thread = new Thread(() =>
        //    {
        //        mainWindow = new MainWindow();
        //        mainWindow.Closed += (s, e) => mainWindow.Dispatcher.InvokeShutdown();
        //        mainWindow.Show();
        //        System.Windows.Threading.Dispatcher.Run();
        //    });
        //    thread.SetApartmentState(ApartmentState.STA); 
        //    thread.Start();
        //    //thread.Join();

        //    var viewModel = new MonteCarloSolverViewModel();
        //    //mainWindow.DataContext = viewModel;
            
        //    // this execution of ExecuteMonteCarloSolverCommand errors in "try"
        //    // because threading not established, so values are as initialized
        //    viewModel.ExecuteMonteCarloSolverCommand.Execute(null);
        //    Assert.That(viewModel.CanRunSimulation, Is.True);
        //    Assert.That(viewModel.CanLoadInputFile, Is.True);
        //    Assert.That(viewModel.CanCancelSimulation, Is.False);
        //    Assert.That(viewModel.CanSaveResults, Is.True);
        //    Assert.That(viewModel.CancelButtonText == "Cancel", Is.True);
        //}

        ///// <summary>
        ///// verify that 
        ///// </summary>
        //[Test]
        //public void verify_all_infiles_in_download_run_successfully()
        //{
        //    var windowViewModel = new WindowViewModel();
        //    var viewModel = windowViewModel.MonteCarloSolverVM;
        //    foreach (var folder in listOfInfileFolders)
        //    {
        //        string infileName = "infile_" + folder + ".txt";
        //        var simulationInput = SimulationInput.FromFile(infileName);
        //        //specify infile
        //        viewModel.SimulationInputVM.SimulationInput = simulationInput;
        //        // run infile
        //        viewModel.ExecuteMonteCarloSolverCommand.Execute(null);
        //        // verify text output is correct
        //        TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
        //        Assert.That("", Is.EqualTo(textOutputViewModel.Text));
        //    }

        //}
    }
}

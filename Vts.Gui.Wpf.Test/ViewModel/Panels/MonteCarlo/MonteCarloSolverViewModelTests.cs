using NUnit.Framework;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading;
using Vts.Gui.Wpf.View;
using Vts.MonteCarlo;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for MonteCarloSolverViewModelTests
    /// </summary>
    [TestFixture]
    public class MonteCarloSolverViewModelTests
    {

        List<string> listOfInfileFolders = new List<string>()
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
            Assert.IsTrue(viewModel.CanDownloadInfiles);
            Assert.IsTrue(viewModel.CanLoadInputFile);
            Assert.IsTrue(viewModel.CanRunSimulation);
            Assert.IsTrue(viewModel.CanRunSimulation);
            Assert.IsFalse(viewModel.CanCancelSimulation);
            Assert.IsFalse(viewModel.CanSaveResults);
            Assert.AreEqual(viewModel.CancelButtonText,"Cancel");
        }

        ///// <summary>
        ///// Write an infile to be read and verify MC_ReadSimulationInputFromFile method runs successfully.
        ///// </summary>
        //[Test]
        //public void validate_MC_ReadSimulationInputFromFile_runs_successfully()
        //{
        //    MonteCarloSolverViewModel viewModel = new MonteCarloSolverViewModel();
        //    // create default simulation input
        //    SimulationInput simulationInput = new SimulationInput();
        //    // write it to file
        //    simulationInput.ToFile("infile_test");
        //    var privateObject = new PrivateObject(viewModel);
        //    var returnedSimulationInput = viewModel.Invoke("MC_ReadSimulationInputFromFile");
        //    // I can't call MC_ReadSimulationInputFromFile because private
        //    Assert.IsTrue(viewModel.CanRunSimulation);
        //    Assert.IsTrue(viewModel.CanLoadInputFile);
        //}

        // the following set of test validate the relay commands

        ///// <summary>
        ///// Load default SimulationInput and verify LoadSimulationInputCommand executes successfully.
        ///// This opens a dialog window so not sure how to code unit test
        ///// </summary>
        //[Test]
        //public void validate_LoadSimulationInputCommand_executes_successfully()
        //{
        //    WindowViewModel windowViewModel = new WindowViewModel();
        //    var viewModel = windowViewModel.MonteCarloSolverVM;
        //    SimulationInput simulationInput = new SimulationInput();
        //    viewModel.SimulationInputVM = new SimulationInputViewModel(simulationInput);
        //    viewModel.LoadSimulationInputCommand.Execute(null);
        //    Assert.IsTrue(viewModel.CanRunSimulation);
        //    Assert.IsTrue(viewModel.CanLoadInputFile);
        //}

        ///// <summary>
        ///// Execute Monte Carlo Solver command with default infile and verify properties are set correctly
        ///// </summary>
        //[Test]
        //public void validate_ExecuteMonteCarloSolverCommand_executes_successfully()
        //{
        //    MainWindow mainWindow = null;
        //    // need to instantiate MainWindow which is needed in main "try"
        //    Thread thread = new Thread(() =>
        //    {
        //        mainWindow = new MainWindow();
        //        //mainWindow.Closed += (s, e) => mainWindow.Dispatcher.InvokeShutdown();
        //        //mainWindow.Show();
        //        //System.Windows.Threading.Dispatcher.Run();
        //    });
        //    thread.ApartmentState = ApartmentState.STA;
        //    thread.Start();
        //    //var mainWindow = new MainWindow();

        //    var viewModel = WindowViewModel.Current.MonteCarloSolverVM;
        //    mainWindow.DataContext = viewModel;

        //    viewModel.ExecuteMonteCarloSolverCommand.Execute(null);
        //    Assert.IsTrue(viewModel.CanRunSimulation);
        //    Assert.IsTrue(viewModel.CanLoadInputFile);
        //    Assert.IsFalse(viewModel.CanCancelSimulation);
        //    Assert.IsTrue(viewModel.CanSaveResults);
        //    Assert.IsTrue(viewModel.CancelButtonText == "Cancel");
        //}

        //[Test]
        //public void verify_all_infiles_in_download_run_successfully()
        //{
        //    MonteCarloSolverViewModel viewModel = new MonteCarloSolverViewModel();
        //    foreach (var folder in listOfInfileFolders)
        //    {
        //        SimulationInput simulationInput = new SimulationInput("infile_" + folder + ".txt");
        //        //specify infile
        //        viewModel.SimulationInputVM(simulationInput);
        //        // load infile
        //        viewModel.LoadSimulationInputCommand.Execute(null);
        //        // run infile
        //        viewModel.ExecuteMonteCarloSolverCommand.Execute(null);
        //        // 
        //        Assert.IsTrue(Directory.Exists())
        //    }

        //}
    }
}

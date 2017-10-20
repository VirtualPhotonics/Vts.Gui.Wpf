using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Vts.Common.Logging;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.IO;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing the Monte Carlo panel functionality (experimental)
    /// </summary>
    public class MonteCarloSolverViewModel : BindableObject
    {
        private const string TEMP_RESULTS_FOLDER = "mc_results_temp";

        private static readonly ILogger logger =
            LoggerFactoryLocator.GetDefaultNLogFactory().Create(typeof(MonteCarloSolverViewModel));

        private CancellationTokenSource _currentCancellationTokenSource;

        private double[] _mapArrayBuffer;

        //private bool _firstTimeSaving;

        private bool _newResultsAvailable;

        private SimulationOutput _output;

        private MonteCarloSimulation _simulation;

        private SimulationInputViewModel _simulationInputVM;

        public MonteCarloSolverViewModel()
        {
            var simulationInput = SimulationInputProvider.PointSourceTwoLayerTissueROfRhoDetector();

            _simulationInputVM = new SimulationInputViewModel(simulationInput);

            var rho =
                ((ROfRhoDetectorInput)
                    simulationInput.DetectorInputs.Where(d => d.TallyType == TallyType.ROfRho).First()).Rho;

            ExecuteMonteCarloSolverCommand = new RelayCommand(() => MC_ExecuteMonteCarloSolver_Executed(null, null));
            CancelMonteCarloSolverCommand = new RelayCommand(() => MC_CancelMonteCarloSolver_Executed(null, null));
            LoadSimulationInputCommand = new RelayCommand(() => MC_LoadSimulationInput_Executed(null, null));
            DownloadDefaultSimulationInputCommand =
                new RelayCommand(() => MC_DownloadDefaultSimulationInput_Executed(null, null));
            SaveSimulationResultsCommand = new RelayCommand(() => MC_SaveSimulationResults_Executed(null, null));

            _newResultsAvailable = false;
        }

        public RelayCommand ExecuteMonteCarloSolverCommand { get; private set; }
        public RelayCommand CancelMonteCarloSolverCommand { get; private set; }
        public RelayCommand LoadSimulationInputCommand { get; private set; }
        public RelayCommand DownloadDefaultSimulationInputCommand { get; private set; }
        public RelayCommand SaveSimulationResultsCommand { get; private set; }

        public SimulationInputViewModel SimulationInputVM
        {
            get { return _simulationInputVM; }
            set
            {
                _simulationInputVM = value;
                OnPropertyChanged("SimulationInputVM");
            }
        }

        private async void MC_ExecuteMonteCarloSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _currentCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var input = _simulationInputVM.SimulationInput;

                MainWindow.Current.Wait.Visibility = Visibility.Visible;
                ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Begin();
                _newResultsAvailable = false;

                var validationResult = SimulationInputValidation.ValidateInput(input);
                if (!validationResult.IsValid)
                {
                    logger.Info(() => "\rSimulation input not valid.\rRule: " + validationResult.ValidationRule +
                                      (!string.IsNullOrEmpty(validationResult.Remarks)
                                          ? "\rDetails: " + validationResult.Remarks
                                          : "") + ".\r");
                    return;
                }

                _simulation = new MonteCarloSimulation(input);

                _output = await Task.Run(() => _simulation.Run(), _currentCancellationTokenSource.Token);
                if (_output != null)
                {
                    _newResultsAvailable = _simulation.ResultsAvailable;

                    var rOfRhoDetectorInputs = _simulationInputVM.SimulationInput.DetectorInputs.
                        Where(di => di.Name == "ROfRho");

                    if (rOfRhoDetectorInputs.Any())
                    {
                        logger.Info(() => "Creating R(rho) plot...");

                        var detectorInput = (ROfRhoDetectorInput)rOfRhoDetectorInputs.First();

                        var independentValues = detectorInput.Rho.AsEnumerable().ToArray();

                        DoubleDataPoint[] points = null;

                        points = independentValues.Zip(_output.R_r,
                            (x, y) => new DoubleDataPoint(x, y)).ToArray();

                        var axesLabels = GetPlotLabels();
                        WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);

                        var plotLabel = GetPlotLabel();
                        WindowViewModel.Current.PlotVM.PlotValues.Execute(new[] { new PlotData(points, plotLabel) });
                        logger.Info(() => "done.\r");
                    }

                    var fluenceDetectorInputs = _simulationInputVM.SimulationInput.DetectorInputs.
                        Where(di => di.Name == "FluenceOfRhoAndZ");

                    if (fluenceDetectorInputs.Any())
                    {
                        logger.Info(() => "Creating Fluence(rho,z) map...");
                        var detectorInput = (FluenceOfRhoAndZDetectorInput)fluenceDetectorInputs.First();
                        var rhosMC = detectorInput.Rho.AsEnumerable().ToArray();
                        var zsMC = detectorInput.Z.AsEnumerable().ToArray();

                        var rhos =
                            rhosMC.Skip(1).Zip(rhosMC.Take(rhosMC.Length - 1),
                                (first, second) => (first + second) / 2).ToArray();
                        var zs =
                            zsMC.Skip(1).Zip(rhosMC.Take(zsMC.Length - 1),
                                (first, second) => (first + second) / 2).ToArray();

                        var dRhos =
                            rhos.Select(rho => 2 * Math.PI * Math.Abs(rho) * detectorInput.Rho.Delta).ToArray();
                        var dZs = zs.Select(z => detectorInput.Rho.Delta).ToArray();

                        if (_mapArrayBuffer == null || _mapArrayBuffer.Length != _output.Flu_rz.Length * 2)
                        {
                            _mapArrayBuffer = new double[_output.Flu_rz.Length * 2];
                        }

                        // flip the array (since it goes over zs and then rhos, while map wants rhos and then zs
                        for (var zi = 0; zi < zs.Length; zi++)
                        {
                            for (var rhoi = 0; rhoi < rhos.Length; rhoi++)
                            {
                                _mapArrayBuffer[rhoi + rhos.Length + rhos.Length * 2 * zi] = _output.Flu_rz[rhoi, zi];
                            }
                            var localRhoiForReverse = 0;
                            for (var rhoi = rhos.Length - 1; rhoi >= 0; rhoi--, localRhoiForReverse++)
                            {
                                _mapArrayBuffer[localRhoiForReverse + rhos.Length * 2 * zi] = _output.Flu_rz[rhoi, zi];
                            }
                        }

                        var twoRhos = rhos.Reverse().Select(rho => -rho).Concat(rhos).ToArray();
                        var twoDRhos = dRhos.Reverse().Concat(dRhos).ToArray();

                        var mapData = new MapData(_mapArrayBuffer, twoRhos, zs, twoDRhos, dZs);

                        WindowViewModel.Current.MapVM.PlotMap.Execute(mapData);
                        logger.Info(() => "done.\r");
                    }
                }
                ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
                MainWindow.Current.Wait.Visibility = Visibility.Hidden;
                await Task.Run(() => MC_SaveTemporaryResults(input), _currentCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
                MainWindow.Current.Wait.Visibility = Visibility.Hidden;
            }
        }

        private async void MC_SaveTemporaryResults(SimulationInput input)
        {
            // save results to bin folder
            logger.Info(() => "Saving simulation results to temporary directory...");

            // create the root directory
            // create the detector directory, removing stale files first if they exist
            FileIO.CreateEmptyDirectory(TEMP_RESULTS_FOLDER);

            // write detector to file
            input.ToFile(Path.Combine(TEMP_RESULTS_FOLDER, "infile_" + input.OutputName + ".txt"));
            foreach (var result in _output.ResultsDictionary.Values)
            {
                // save all detector data to the specified folder
                DetectorIO.WriteDetectorToFile(result, TEMP_RESULTS_FOLDER);
            }
            logger.Info(() => "done.\r");
        }

        private async void MC_CancelMonteCarloSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_simulation != null && _simulation.IsRunning)
            {
                await Task.Run(()=> _simulation.Cancel());
                _currentCancellationTokenSource.Cancel();
                logger.Info(() => "Simulation cancelled.\n");
            }
        }

        private void MC_LoadSimulationInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "TXT Files (*.txt)|*.txt"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dialog.ShowDialog();

            // if the file dialog returns true - file was selected 
            var filename = "";
            if (result == true)
            {
                // Get the filename from the dialog 
                filename = dialog.FileName;
            }
            if (filename != "")
            {
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var simulationInput = FileIO.ReadFromJsonStream<SimulationInput>(stream);

                    var validationResult = SimulationInputValidation.ValidateInput(simulationInput);
                    if (validationResult.IsValid)
                    {
                        _simulationInputVM.SimulationInput = simulationInput;
                        logger.Info(() => "Simulation input loaded.\r");
                    }
                    else
                    {
                        logger.Info(() => "Simulation input not loaded - JSON format not valid.\r");
                    }
                }
            }
            else
            {
                logger.Info(() => "JSON File not loaded.\r");
            }
        }

        private void MC_DownloadDefaultSimulationInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Create SaveFileDialog 
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".zip",
                Filter = "ZIP Files (*.zip)|*.zip"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dialog.ShowDialog();

            // if the file dialog returns true - file was selected 
            var filename = "";
            if (result == true)
            {
                // Get the filename from the dialog 
                filename = dialog.FileName;
            }
            if (filename != "")
            {
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    var files = SimulationInputProvider.GenerateAllSimulationInputs().Select(input =>
                        new
                        {
                            Name = "infile_" + input.OutputName + ".txt",
                            Input = input
                        });
 
                    foreach (var file in files)
                    {
                        file.Input.ToFile(file.Name);
                    }
                    FileIO.ZipFiles(files.Select(file => file.Name), "", stream);
                    logger.Info(() => "Template simulation input files exported to a zip file.\r");
                }
            }
        }

        private void MC_SaveSimulationResults_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_output != null && _newResultsAvailable)
            {
                // Create SaveFileDialog 
                var dialog = new SaveFileDialog
                {
                    DefaultExt = ".zip",
                    Filter = "Zip Files (*.zip)|*.zip"
                };

                // Display OpenFileDialog by calling ShowDialog method 
                var result = dialog.ShowDialog();

                // if the file dialog returns true - file was selected 
                var filename = "";
                if (result == true)
                {
                    // Get the filename from the dialog 
                    filename = dialog.FileName;
                }
                if (filename == "") return;
                var input = _simulationInputVM.SimulationInput;
                if (Directory.Exists(TEMP_RESULTS_FOLDER))
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    // copy the MATLAB files to isolated storage and get their names so they can be included in the zip file
                    var matlabFiles = FileIO.CopyFolderFromEmbeddedResources("Matlab", TEMP_RESULTS_FOLDER, currentAssembly.FullName, false);
                    var adjustedMatlabFilenames = new List<string>();
                    foreach (var fileName in matlabFiles)
                    {
                        adjustedMatlabFilenames.Add(Path.Combine(TEMP_RESULTS_FOLDER, fileName));
                    }
                    // get all the files we want to zip up
                    var fileNames = Directory.GetFiles(TEMP_RESULTS_FOLDER);
                    // then, zip all the files together and store *that* .zip to default bin folder
                    var allFiles = adjustedMatlabFilenames.Concat(fileNames).Distinct();
                    try
                    {
                        FileIO.ZipFiles(allFiles, "", input.OutputName + ".zip");
                    }
                    catch (SecurityException)
                    {
                        logger.Error(() => "\rProblem saving results to file.\r");
                    }
                } 
                try
                {
                    using (var stream = new FileStream(filename, FileMode.Create))
                    {
                        using (var zipStream = StreamFinder.GetFileStream(input.OutputName + ".zip", FileMode.Open))
                        {
                            FileIO.CopyStream(zipStream, stream);
                        }
                    }
                    logger.Info(() => "Finished copying results to user file.\r");
                }
                catch (SecurityException)
                {
                    logger.Error(() => "Problem exporting results to user file...sorry user :(\r");
                }
            }
        }

        private PlotAxesLabels GetPlotLabels()
        {
            //return new PlotAxesLabels(
            //    IndependentVariableAxis.Rho.GetInternationalizedString(),
            //    IndependentVariableAxisUnits.MM.GetInternationalizedString(),
            //    IndependentVariableAxis.Rho,
            //    SolutionDomainType.ROfRho.GetInternationalizedString(),
            //    DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString());
            var rhoRange = (ROfRhoDetectorInput) _simulationInputVM.SimulationInput.DetectorInputs.FirstOrDefault();

            var axisType = IndependentVariableAxis.Rho;
            var axisUnits = IndependentVariableAxisUnits.MM;
            return new PlotAxesLabels(
                SolutionDomainType.ROfRho.GetInternationalizedString(),
                DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString(),
                new IndependentAxisViewModel
                {
                    AxisType = axisType,
                    AxisLabel = axisType.GetInternationalizedString(),
                    AxisUnits = axisUnits.GetInternationalizedString(),
                    AxisRangeVM =
                        new RangeViewModel(rhoRange.Rho, axisUnits.GetInternationalizedString(), axisType, "ROfRho")
                });
        }

        private string GetPlotLabel()
        {
            var nString = "N: " + _simulationInputVM.SimulationInput.N;
            var awtString = "AWT: ";
            switch (_simulationInputVM.SimulationInput.Options.AbsorptionWeightingType)
            {
                case AbsorptionWeightingType.Analog:
                    awtString += "analog";
                    break;
                case AbsorptionWeightingType.Discrete:
                    awtString += "discrete";
                    break;
                case AbsorptionWeightingType.Continuous:
                    awtString += "continuous";
                    break;
            }

            return "Model - MC\r" + nString + "\r" + awtString + "";
        }
    }
}
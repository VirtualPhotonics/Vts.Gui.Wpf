using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using Vts;
using Vts.Common.Logging;
using Vts.Gui.Wpf.Input;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.IO;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary> 
    /// View model implementing the Monte Carlo panel functionality (experimental)
    /// </summary>
    public class MonteCarloSolverViewModel : BindableObject
    {
        private const string TEMP_RESULTS_FOLDER = "mc_results_temp";

        private static ILogger logger = LoggerFactoryLocator.GetDefaultNLogFactory().Create(typeof(MonteCarloSolverViewModel));

        private MonteCarloSimulation _simulation;

        private SimulationOutput _output;

        private SimulationInputViewModel _simulationInputVM;

        private CancellationTokenSource _currentCancellationTokenSource;

        //private bool _firstTimeSaving;

        private bool _newResultsAvailable;

        private double[] _mapArrayBuffer;

        public MonteCarloSolverViewModel()
        {
            var simulationInput = SimulationInputProvider.PointSourceTwoLayerTissueROfRhoDetector();

            _simulationInputVM = new SimulationInputViewModel(simulationInput);

            var rho = ((ROfRhoDetectorInput)simulationInput.DetectorInputs.Where(d => d.TallyType == TallyType.ROfRho).First()).Rho;

            ExecuteMonteCarloSolverCommand = new RelayCommand(() => MC_ExecuteMonteCarloSolver_Executed(null, null));
            CancelMonteCarloSolverCommand = new RelayCommand(() => MC_CancelMonteCarloSolver_Executed(null, null));
            LoadSimulationInputCommand = new RelayCommand(() => MC_LoadSimulationInput_Executed(null, null));
            DownloadDefaultSimulationInputCommand = new RelayCommand(() => MC_DownloadDefaultSimulationInput_Executed(null, null));
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

        private void MC_ExecuteMonteCarloSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //if (!EnoughRoomInIsolatedStorage(50))
            //{
            //    logger.Info(() => "\rSimulation not run. Please allocate more than 50MB of storage space.\r");
            //    Commands.IsoStorage_IncreaseSpaceQuery.Execute();
            //    return;
            //}

            _newResultsAvailable = false;

            var input = _simulationInputVM.SimulationInput;


            var validationResult = SimulationInputValidation.ValidateInput(input);
            if (!validationResult.IsValid)
            {
                logger.Info(() => "\rSimulation input not valid.\rRule: " + validationResult.ValidationRule +
                    (!string.IsNullOrEmpty(validationResult.Remarks) ? "\rDetails: " + validationResult.Remarks : "") + ".\r");
                return;
            }

            _simulation = new MonteCarloSimulation(input);

            _currentCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = _currentCancellationTokenSource.Token;
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            var t = Task.Factory.StartNew(() => _simulation.Run(), TaskCreationOptions.LongRunning);
            //need to figure out how to replace all this code
            Action<Task<SimulationOutput>, object> mCAction = (antecedent, obj) =>
            {
                MainWindow.Current.Dispatcher.BeginInvoke((Action)delegate()
                {
                    _output = antecedent.Result;
                    _newResultsAvailable = _simulation.ResultsAvailable;

                    var rOfRhoDetectorInputs = _simulationInputVM.SimulationInput.DetectorInputs.
                        Where(di => di.Name == "ROfRho");

                    if (rOfRhoDetectorInputs.Any())
                    {
                        logger.Info(() => "Creating R(rho) plot...");

                        var detectorInput = (ROfRhoDetectorInput) rOfRhoDetectorInputs.First();

                        double[] independentValues = detectorInput.Rho.AsEnumerable().ToArray();

                        DoubleDataPoint[] points = null;

                        //var showPlusMinusStdev = true;
                        //if(showPlusMinusStdev && _output.R_r2 != null)
                        //{
                        //    var stdev = Enumerable.Zip(_output.R_r, _output.R_r2, (r, r2) => Math.Sqrt((r2 - r * r) / nPhotons)).ToArray();
                        //    var rMinusStdev = Enumerable.Zip(_output.R_r, stdev, (r,std) => r-std).ToArray();
                        //    var rPlusStdev = Enumerable.Zip(_output.R_r, stdev, (r,std) => r+std).ToArray();
                        //    points = Enumerable.Zip(
                        //        independentValues.Concat(independentValues).Concat(independentValues),
                        //        rMinusStdev.Concat(_output.R_r).Concat(rPlusStdev),
                        //        (x, y) => new Point(x, y));
                        //}
                        //else
                        //{
                        points = Enumerable.Zip(
                            independentValues,
                            _output.R_r,
                            (x, y) => new DoubleDataPoint(x, y)).ToArray();
                        //}

                        PlotAxesLabels axesLabels = GetPlotLabels();
                        WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);
                        //Commands.Plot_SetAxesLabels.Execute(axesLabels, null);

                        string plotLabel = GetPlotLabel();
                        WindowViewModel.Current.PlotVM.PlotValues.Execute(new[] {new PlotData(points, plotLabel)});
                        //Commands.Plot_PlotValues.Execute(new[] { new PlotData(points, plotLabel) }, null);
                        logger.Info(() => "done.\r");
                    }

                    var fluenceDetectorInputs = _simulationInputVM.SimulationInput.DetectorInputs.
                        Where(di => di.Name == "FluenceOfRhoAndZ");

                    if (fluenceDetectorInputs.Any())
                    {
                        logger.Info(() => "Creating Fluence(rho,z) map...");
                        var detectorInput = (FluenceOfRhoAndZDetectorInput) fluenceDetectorInputs.First();
                        var rhosMC = detectorInput.Rho.AsEnumerable().ToArray();
                        var zsMC = detectorInput.Z.AsEnumerable().ToArray();

                        var rhos =
                            Enumerable.Zip(rhosMC.Skip(1), rhosMC.Take(rhosMC.Length - 1),
                                (first, second) => (first + second)/2).ToArray();
                        var zs =
                            Enumerable.Zip(zsMC.Skip(1), rhosMC.Take(zsMC.Length - 1),
                                (first, second) => (first + second)/2).ToArray();

                        var dRhos =
                            Enumerable.Select(rhos, rho => 2*Math.PI*Math.Abs(rho)*detectorInput.Rho.Delta).ToArray();
                        var dZs = Enumerable.Select(zs, z => detectorInput.Rho.Delta).ToArray();

                        if (_mapArrayBuffer == null || _mapArrayBuffer.Length != _output.Flu_rz.Length*2)
                        {
                            _mapArrayBuffer = new double[_output.Flu_rz.Length*2];
                        }

                        // flip the array (since it goes over zs and then rhos, while map wants rhos and then zs
                        for (int zi = 0; zi < zs.Length; zi++)
                        {
                            for (int rhoi = 0; rhoi < rhos.Length; rhoi++)
                            {
                                _mapArrayBuffer[rhoi + rhos.Length + rhos.Length*2*zi] = _output.Flu_rz[rhoi, zi];
                            }
                            var localRhoiForReverse = 0;
                            for (int rhoi = rhos.Length - 1; rhoi >= 0; rhoi--, localRhoiForReverse++)
                            {
                                _mapArrayBuffer[localRhoiForReverse + rhos.Length*2*zi] = _output.Flu_rz[rhoi, zi];
                            }
                        }

                        var twoRhos = Enumerable.Concat(rhos.Reverse().Select(rho => -rho), rhos).ToArray();
                        var twoDRhos = Enumerable.Concat(dRhos.Reverse(), dRhos).ToArray();

                        var mapData = new MapData(_mapArrayBuffer, twoRhos, zs, twoDRhos, dZs);

                        //Commands.Maps_PlotMap.Execute(mapData, null);
                        WindowViewModel.Current.MapVM.PlotMap.Execute(mapData);
                        logger.Info(() => "done.\r");
                    }

                    // save results to isolated storage
                    logger.Info(() => "Saving simulation results to temporary directory...");
                    //var detectorFolder = Path.Combine(TEMP_RESULTS_FOLDER, input.OutputName);

                    //// create the root directory
                    //FileIO.CreateDirectory(TEMP_RESULTS_FOLDER);
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

                });
            };
            var c = t.ContinueWith(mCAction, null, cancelToken, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
        }

        private void MC_CancelMonteCarloSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentCancellationTokenSource != null)
            {
                _currentCancellationTokenSource.Cancel(true);
                _currentCancellationTokenSource = null;
                //logger.Info(() => "Simulation cancelled.\n");
            }
            if (_simulation != null && _simulation.IsRunning)
            {
                Task.Factory.StartNew(() => _simulation.Cancel());
            }
        }

        private void MC_LoadSimulationInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "TXT Files (*.txt)|*.txt"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dialog.ShowDialog();
            
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
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".zip",
                Filter = "ZIP Files (*.zip)|*.zip"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dialog.ShowDialog();
            
            // if the file dialog returns true - file was selected 
            var filename = "";
            if (result == true)
            {
                // Get the filename from the dialog 
                filename = dialog.FileName;
            }
            if (filename != "")
            {
            }
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
                var allFiles = files.Concat(files);
                FileIO.ZipFiles(files.Select(file => file.Name), "", stream);
                logger.Info(() => "Template simulation input files exported to a zip file.\r");
            }
        }

        private void MC_SaveSimulationResults_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_output != null && _newResultsAvailable)
            {
                // Create SaveFileDialog 
                var dialog = new Microsoft.Win32.SaveFileDialog
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
                try
                {
                    using (var stream = new FileStream(filename, FileMode.Create))
                    {
                        using (var zipStream = StreamFinder.GetFileStream(input.OutputName + ".zip", FileMode.Open))
                        {
                            FileIO.CopyStream(stream, zipStream);
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

        private bool EnoughRoomInIsolatedStorage(double megabyteMinimum)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var currentQuota = (isf.Quota / 1048576);
                var spaceUsed = ((isf.Quota - isf.AvailableFreeSpace) / 1048576);
                if (currentQuota - spaceUsed < megabyteMinimum)
                {
                    return false;
                }
            }
            return true;
        }

        private PlotAxesLabels GetPlotLabels()
        {
            //return new PlotAxesLabels(
            //    IndependentVariableAxis.Rho.GetInternationalizedString(),
            //    IndependentVariableAxisUnits.MM.GetInternationalizedString(),
            //    IndependentVariableAxis.Rho,
            //    SolutionDomainType.ROfRho.GetInternationalizedString(),
            //    DependentVariableAxisUnits.PerMMSquared.GetInternationalizedString());
            var rhoRange = (ROfRhoDetectorInput)_simulationInputVM.SimulationInput.DetectorInputs.FirstOrDefault();
            
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
                    AxisRangeVM = new RangeViewModel(rhoRange.Rho, axisUnits.GetInternationalizedString(), axisType, "ROfRho")
                });
        }

        private string GetPlotLabel()
        {
            string nString = "N: " + _simulationInputVM.SimulationInput.N;
            string awtString = "AWT: ";
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

﻿using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Vts.Common.Logging;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.IO;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing the Monte Carlo panel functionality (experimental)
    /// </summary>
    public class MonteCarloSolverViewModel : BindableObject
    {
        private static readonly ILogger logger =
            LoggerFactoryLocator.GetDefaultNLogFactory().Create(typeof(MonteCarloSolverViewModel));

        private CancellationTokenSource _currentCancellationTokenSource;

        private double[] _mapArrayBuffer;

        private string _outputName;

        private bool _newResultsAvailable;
        private bool _canRunSimulation;
        private bool _canCancelSimulation;
        private bool _canSaveResults;
        private bool _canLoadInputFile;
        private bool _canDownloadInfiles;

        private string _cancelButtonText;

        private SimulationOutput _output;
        private readonly ObjectCache _cache = MemoryCache.Default;

        private MonteCarloSimulation _simulation;

        private SimulationInputViewModel _simulationInputVm;

        public MonteCarloSolverViewModel()
        {
            var simulationInput = SimulationInputProvider.PointSourceTwoLayerTissueROfRhoDetector();

            _simulationInputVm = new SimulationInputViewModel(simulationInput);
            _outputName = simulationInput.OutputName;

            ExecuteMonteCarloSolverCommand = new RelayCommand(() => MC_ExecuteMonteCarloSolver_Executed(null, null));
            CancelMonteCarloSolverCommand = new RelayCommand(() => MC_CancelMonteCarloSolver_Executed(null, null));
            LoadSimulationInputCommand = new RelayCommand(() => MC_LoadSimulationInput_Executed(null, null));
            DownloadDefaultSimulationInputCommand =
                new RelayCommand(() => MC_DownloadDefaultSimulationInput_Executed(null, null));
            SaveSimulationResultsCommand = new RelayCommand(() => MC_SaveSimulationResults_Executed(null, null));

            _canDownloadInfiles = true;
            _canLoadInputFile = true;
            _canRunSimulation = true;
            _canCancelSimulation = false;
            _canSaveResults = false;
            _newResultsAvailable = false;
            _cancelButtonText = StringLookup.GetLocalizedString("Button_Cancel");
        }

        public RelayCommand ExecuteMonteCarloSolverCommand { get; private set; }
        public RelayCommand CancelMonteCarloSolverCommand { get; private set; }
        public RelayCommand LoadSimulationInputCommand { get; private set; }
        public RelayCommand DownloadDefaultSimulationInputCommand { get; private set; }
        public RelayCommand SaveSimulationResultsCommand { get; private set; }

        public string CancelButtonText
        {
            get => _cancelButtonText;
            set
            {
                _cancelButtonText = value;
                OnPropertyChanged("CancelButtonText");
            }
        }

        public bool CanDownloadInfiles
        {
            get => _canDownloadInfiles;
            set
            {
                _canDownloadInfiles = value;
                OnPropertyChanged("CanDownloadInfiles");
            }
        }

        public bool CanRunSimulation
        {
            get => _canRunSimulation;
            set
            {
                _canRunSimulation = value;
                OnPropertyChanged("CanRunSimulation");
            }
        }

        public bool CanCancelSimulation
        {
            get => _canCancelSimulation;
            set
            {
                _canCancelSimulation = value;
                OnPropertyChanged("CanCancelSimulation");
            }
        }

        public bool CanSaveResults
        {
            get => _canSaveResults;
            set
            {
                _canSaveResults = value;
                OnPropertyChanged("CanSaveResults");
            }
        }

        public bool CanLoadInputFile
        {
            get => _canLoadInputFile;
            set
            {
                _canLoadInputFile = value;
                OnPropertyChanged("CanLoadInputFile");
            }
        }

        public SimulationInputViewModel SimulationInputVM
        {
            get => _simulationInputVm;
            set
            {
                _simulationInputVm = value;
                OnPropertyChanged("SimulationInputVM");
            }
        }

        private async void MC_ExecuteMonteCarloSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _currentCancellationTokenSource = new CancellationTokenSource();
            CanRunSimulation = false;
            CanLoadInputFile = false;
            CanCancelSimulation = true;
            CancelButtonText = StringLookup.GetLocalizedString("Button_CancelSimulation");
            CanSaveResults = false;
            var mapView = false;
            var plotView = false;

            //clear the map in case there is no new mapview
            WindowViewModel.Current.MapVM.ClearMap.Execute(null);

            try
            {
                var input = _simulationInputVm.SimulationInput;

                if (MainWindow.Current != null)
                {
                    MainWindow.Current.Wait.Visibility = Visibility.Visible;
                    ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Begin();
                }
                _newResultsAvailable = false;

                var validationResult = SimulationInputValidation.ValidateInput(input);
                if (!validationResult.IsValid)
                {
                    logger.Info(() => StringLookup.GetLocalizedString("Message_InvalidSimulationInput") +
                                      "\r" + StringLookup.GetLocalizedString("Message_Rule") + validationResult.ValidationRule +
                                      (!string.IsNullOrEmpty(validationResult.Remarks)
                                          ? "\r" + StringLookup.GetLocalizedString("Message_Details") + validationResult.Remarks
                                          : "") + ".\r");
                    return;
                }

                if (!MC_InfileIsValidForGUI(input))
                {
                    return;
                }

                _simulation = new MonteCarloSimulation(input);

                _output = await Task.Run(() => _simulation.Run(), _currentCancellationTokenSource.Token);
                if (_output != null)
                {
                    _newResultsAvailable = _simulation.ResultsAvailable;

                    var rOfRhoDetectorInputs =
                        _simulationInputVm.SimulationInput.DetectorInputs.Where(di => di.Name == "ROfRho");

                    if (rOfRhoDetectorInputs.Any())
                    {
                        logger.Info(() => StringLookup.GetLocalizedString("Message_CreateROfRhoPlot"));

                        var detectorInput = (ROfRhoDetectorInput) rOfRhoDetectorInputs.First();

                        var independentValues = detectorInput.Rho.AsEnumerable().ToArray();

                        DoubleDataPoint[] points = null;

                        points = independentValues.Zip(_output.R_r,
                            (x, y) => new DoubleDataPoint(x, y)).ToArray();

                        var axesLabels = GetPlotLabels();
                        if (MainWindow.Current != null)
                            WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);

                        var plotLabel = GetPlotLabel();
                        if (MainWindow.Current != null)
                            WindowViewModel.Current.PlotVM.PlotValues.Execute(new[] {new PlotData(points, plotLabel)});
                        plotView = true;
                        logger.Info(() => StringLookup.GetLocalizedString("Message_Done") + ".\r");
                    }

                    var fluenceDetectorInputs =
                        _simulationInputVm.SimulationInput.DetectorInputs.Where(di => di.Name == "FluenceOfRhoAndZ");

                    if (fluenceDetectorInputs.Any())
                    {
                        logger.Info(() => StringLookup.GetLocalizedString("Message_CreatingFluenceRhoZMap"));
                        var detectorInput = (FluenceOfRhoAndZDetectorInput) fluenceDetectorInputs.First();
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
                        mapView = true;
                        logger.Info(() => StringLookup.GetLocalizedString("Message_Done") + ".\r");
                    }
                    // put map view on top if no plot and plot view on top if no map otherwise stay on current view
                    if (!(plotView && mapView) && MainWindow.Current != null)
                    {
                        MainWindow.Current.Main_SelectView_Executed(rOfRhoDetectorInputs.Any() ? 0 : 1);
                    }
                }

                if (MainWindow.Current != null)
                {
                    ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
                    MainWindow.Current.Wait.Visibility = Visibility.Hidden;
                }
                await Task.Run(() => MC_CacheSimulationResults(input), _currentCancellationTokenSource.Token);
                CanRunSimulation = true;
                CanLoadInputFile = true;
                CancelButtonText = StringLookup.GetLocalizedString("Button_Cancel");
                CanCancelSimulation = false;
                CanSaveResults = true;
            }
            catch (OperationCanceledException)
            {
                if (MainWindow.Current != null)
                {
                    ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
                    MainWindow.Current.Wait.Visibility = Visibility.Hidden;
                }
            }
            finally
            {
                if (MainWindow.Current != null)
                {
                    ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
                    MainWindow.Current.Wait.Visibility = Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// This method goes beyond the regular SimulationInput validation performed in SimulationInputValidation class.
        /// The validation performed here checks that the input can be plotted, i.e., tissue and detectors have 
        /// cylindrical symmetry.  It also checks if a database output is specified and specifies that a new infile
        /// be specified because database files cannot be cached. Finally it checks if TrackStatistics is true
        /// and if so, logs to the user to verify selection using TrackStatistics check box 
        /// </summary>
        /// <param name="input">Simulation input</param>
        private bool MC_InfileIsValidForGUI(SimulationInput input)
        {
            var infileIsValid = true;
            if ((!(input.TissueInput is MultiLayerTissueInput)) && (!(input.TissueInput is SingleEllipsoidTissueInput)))
            {
                logger.Info(() => StringLookup.GetLocalizedString("Warning_NoPlotsDisplayedForTissue") + ".\r");
            }
            if ((input.DetectorInputs.All(d => d.TallyType != TallyType.ROfRho)) &&
                (input.DetectorInputs.All(d => d.TallyType != TallyType.FluenceOfRhoAndZ)))  
            {
                logger.Info(() => StringLookup.GetLocalizedString("Warning_NoPlotsDisplayedForDetector") + ".\r");
            }
            if (input.Options.Databases.Count != 0)
            {
                logger.Info(() => StringLookup.GetLocalizedString("Error_DatabaseOutputNotSupported") + ".\r");
                infileIsValid = false;
            }
            return infileIsValid;
        }

        private void MC_CacheSimulationResults(SimulationInput input)
        {
            // cache the simulation results
            logger.Info(() => StringLookup.GetLocalizedString("Message_CachingSimulationResults"));

            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromHours(2)
            };

            _cache.Set("SimulationInput", input, policy);
            _cache.Set("SimulationOutput", _output, policy);
            logger.Info(() => StringLookup.GetLocalizedString("Message_Done") + ".\r");
        }

        private async Task<bool> MC_SaveSimulationResultsFromCache()
        {
            var input = _cache["SimulationInput"] as SimulationInput;
            var output = _cache["SimulationOutput"] as SimulationOutput;
            if (input == null || output == null) return false;
            using (var dialog = new FolderBrowserDialog())
            {
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    // create the root directory
                    try
                    {
                        var folder = Path.Combine(dialog.SelectedPath, "results");
                        if (Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any())
                        {
                            // there are files in this directory, ok to delete them?
                            var messageBoxResult =
                                System.Windows.MessageBox.Show(
                                    StringLookup.GetLocalizedString("MessageBox_DeleteConfirmResults"),
                                    StringLookup.GetLocalizedString("MessageBoxTitle_DeleteConfirm"), MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.No)
                            {
                                return false;
                            }
                        }
                        logger.Info(() => StringLookup.GetLocalizedString("Message_SaveSimulationResults"));
                        CanSaveResults = false;
                        CanRunSimulation = false;
                        CanLoadInputFile = false;
                        CanCancelSimulation = true;
                        CancelButtonText = StringLookup.GetLocalizedString("Button_CancelSave");
                        await Task.Run(() => MC_SaveSimulationResultsToFolder(input, output, folder),
                            _currentCancellationTokenSource.Token);
                        CanSaveResults = true;
                        CanRunSimulation = true;
                        CanLoadInputFile = true;
                        CancelButtonText = StringLookup.GetLocalizedString("Button_Cancel");
                        CanCancelSimulation = false;

                        logger.Info(() => StringLookup.GetLocalizedString("Message_Done") + ".\r");
                        return true;
                    }
                    catch (Exception)
                    {
                        logger.Info(() => StringLookup.GetLocalizedString("Error_FileSave"));
                        return false;
                    }
                }
            }
            return false;
        }

        private void MC_SaveSimulationResultsToFolder(SimulationInput input, SimulationOutput output, string folder)
        {
            // the saving of files to a "results" folder is organized so that the user would just have to edit load_results_script
            // with their particular OutputName and run it
            FileIO.CreateEmptyDirectory(folder);  // create "results" folder for all saved files
            var resultsSubfolder = Path.Combine(folder, _outputName); // create "results" subfolder that will hold detector results
            FileIO.CreateEmptyDirectory(resultsSubfolder); 

            // write detector results and infile to file
            input.ToFile(Path.Combine(resultsSubfolder, _outputName + ".txt"));  // write infile with infile.OutputName
            foreach (var result in output.ResultsDictionary.Values)
            {
                // save all detector data to the specified folder
                DetectorIO.WriteDetectorToFile(result, resultsSubfolder);
            }
            // copy the MATLAB files to the "results" folder also
            var currentAssembly = Assembly.GetExecutingAssembly();
            FileIO.CopyFolderFromEmbeddedResources("Matlab", folder, currentAssembly.FullName, false); // put Matlab scripts in "results"
        }

        private async void MC_CancelMonteCarloSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CanCancelSimulation = false;
            CanRunSimulation = true;
            CanLoadInputFile = true;
            if (_simulation != null && _simulation.IsRunning)
            {
                await Task.Run(() => _simulation.Cancel());
            }
            _currentCancellationTokenSource.Cancel();
            logger.Info(() => StringLookup.GetLocalizedString("Message_Cancelled") + ".\n");
        }

        private async void MC_LoadSimulationInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "TXT Files (*.txt)|*.txt"
            };

            CanRunSimulation = false;
            CanLoadInputFile = false;
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
                var simulationInput = await Task.Run(() => MC_ReadSimulationInputFromFile(filename));
                if (simulationInput != null)
                {
                    _outputName = simulationInput.OutputName;
                    SimulationInputVM.SimulationInput = simulationInput;
                    CanRunSimulation = true;
                    CanLoadInputFile = true;
                    if (simulationInput.Options.TrackStatistics)
                    {
                        _simulationInputVm.SimulationOptionsVM.SetStatisticsFolderCommand.Execute(null);
                    }
                }
            }
            else
            {
                logger.Info(() => StringLookup.GetLocalizedString("Error_FileLoad") + ".\r");
            }
            CanRunSimulation = true;
            CanLoadInputFile = true;
        }

        private SimulationInput MC_ReadSimulationInputFromFile(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var simulationInput = FileIO.ReadFromJsonStream<SimulationInput>(stream);

                var validationResult = SimulationInputValidation.ValidateInput(simulationInput);
                if (validationResult.IsValid && MC_InfileIsValidForGUI(simulationInput))
                {
                    logger.Info(() => StringLookup.GetLocalizedString("Message_SimulationInputLoaded") + "\r");
                    return simulationInput;
                }
                logger.Info(() => StringLookup.GetLocalizedString("Error_SimulationInputNotLoaded") + "\r"); 
                return null;
            }
        }

        private async void MC_DownloadDefaultSimulationInput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    // create the root directory
                    try
                    {
                        var folder = Path.Combine(dialog.SelectedPath, "infiles");
                        if (Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any())
                        {
                            // there are files in this directory, ok to delete them?
                            var messageBoxResult =
                                System.Windows.MessageBox.Show(
                                    StringLookup.GetLocalizedString("MessageBox_DeleteConfirmInfiles"),
                                    StringLookup.GetLocalizedString("MessageBoxTitle_DeleteConfirm"), MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                        logger.Info(() => StringLookup.GetLocalizedString("Message_DownloadingInfiles"));
                        CanDownloadInfiles = false;
                        await Task.Run(() => MC_DownloadDefaultSimulationInputToFolder(folder));
                        CanDownloadInfiles = true;
                        logger.Info(() => StringLookup.GetLocalizedString("Message_Done") + ".\r");
                    }
                    catch (Exception)
                    {
                        logger.Info(() => StringLookup.GetLocalizedString("Error_FileSave"));
                    }
                }
            }
        }

        private void MC_DownloadDefaultSimulationInputToFolder(string folder)
        {
            FileIO.CreateEmptyDirectory(folder);
            var files = SimulationInputProvider.GenerateAllSimulationInputs().Select(input =>
                new
                {
                    Name = "infile_" + input.OutputName + ".txt",
                    Input = input
                });

            foreach (var file in files)
            {
                file.Input.ToFile(Path.Combine(folder, file.Name));
            }
        }

        private async void MC_SaveSimulationResults_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_output != null && _newResultsAvailable)
            {
                var result = await MC_SaveSimulationResultsFromCache();
                if (!result)
                {
                    logger.Info(() => StringLookup.GetLocalizedString("Error_FileSave"));
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

            //var rhoRange = (ROfRhoDetectorInput) _simulationInputVM.SimulationInput.DetectorInputs.FirstOrDefault();
            var rOfRhoDetectorInputs =
                _simulationInputVm.SimulationInput.DetectorInputs.Where(di => di.Name == "ROfRho");
            var detectorInput = (ROfRhoDetectorInput)rOfRhoDetectorInputs.First();
            var rhoRange = detectorInput.Rho;

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
                        new RangeViewModel(rhoRange, axisUnits.GetInternationalizedString(), axisType, "ROfRho")
                });
        }

        private string GetPlotLabel()
        {
            var nString = StringLookup.GetLocalizedString("PlotLabel_N") + _simulationInputVm.SimulationInput.N;
            var awtString = StringLookup.GetLocalizedString("PlotLabel_AWT");
            switch (_simulationInputVm.SimulationInput.Options.AbsorptionWeightingType)
            {
                case AbsorptionWeightingType.Analog:
                    awtString += StringLookup.GetLocalizedString("Label_Analog");
                    break;
                case AbsorptionWeightingType.Discrete:
                    awtString += StringLookup.GetLocalizedString("Label_Discrete");
                    break;
                case AbsorptionWeightingType.Continuous:
                    awtString += StringLookup.GetLocalizedString("Label_Continuous");
                    break;
            }

            return StringLookup.GetLocalizedString("Label_ModelMC") + "\r" + nString + "\r" + awtString + "";
        }
    }
}
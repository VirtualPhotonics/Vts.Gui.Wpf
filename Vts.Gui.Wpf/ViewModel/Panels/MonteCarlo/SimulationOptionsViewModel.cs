using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using Vts.Gui.Wpf.Extensions;
using Vts.MonteCarlo;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel;

public class SimulationOptionsViewModel : BindableObject
{
    private OptionViewModel<AbsorptionWeightingType> _absorptionWeightingTypeVM;
    private OptionViewModel<PhaseFunctionType> _phaseFunctionTypeVM;

    public RelayCommand SetStatisticsFolderCommand { get; private set; }

    private OptionViewModel<RandomNumberGeneratorType> _randomNumberGeneratorTypeVM;
    private SimulationOptions _simulationOptions;

    public SimulationOptionsViewModel(SimulationOptions options)
    {
        _simulationOptions = options; // use the property to invoke the appropriate change notification

#if WHITELIST 
        _absorptionWeightingTypeVM = new OptionViewModel<AbsorptionWeightingType>("Absorption Weighting Type:", false, _simulationOptions.AbsorptionWeightingType, WhiteList.AbsorptionWeightingTypes);
        _randomNumberGeneratorTypeVM = new OptionViewModel<RandomNumberGeneratorType>("Random Number Generator Type:", false, _simulationOptions.RandomNumberGeneratorType, WhiteList.RandomNumberGeneratorTypes);
        _phaseFunctionTypeVM = new OptionViewModel<PhaseFunctionType>("Phase Function Type:", false, _simulationOptions.PhaseFunctionType, WhiteList.PhaseFunctionTypes);
#else
        _absorptionWeightingTypeVM = new OptionViewModel<AbsorptionWeightingType>("Absorption Weighting Type:",
            false, _simulationOptions.AbsorptionWeightingType);
        _randomNumberGeneratorTypeVM = new OptionViewModel<RandomNumberGeneratorType>("Random Number Generator:",
            false, _simulationOptions.RandomNumberGeneratorType);
        _phaseFunctionTypeVM = new OptionViewModel<PhaseFunctionType>("Phase Function Type:", false,
            _simulationOptions.PhaseFunctionType);
#endif
        SetStatisticsFolderCommand = new RelayCommand(() => MC_SetStatisticsFolder_Executed(null, null));

        _absorptionWeightingTypeVM.PropertyChanged += (sender, args) =>
            _simulationOptions.AbsorptionWeightingType = _absorptionWeightingTypeVM.SelectedValue;
        _randomNumberGeneratorTypeVM.PropertyChanged += (sender, args) =>
            _simulationOptions.RandomNumberGeneratorType = _randomNumberGeneratorTypeVM.SelectedValue;
        _phaseFunctionTypeVM.PropertyChanged += (sender, args) =>
            _simulationOptions.PhaseFunctionType = _phaseFunctionTypeVM.SelectedValue;
    }
    public SimulationOptionsViewModel() : this(new SimulationOptions())
    {
    }

    private void MC_SetStatisticsFolder_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (TrackStatistics)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = StringLookup.GetLocalizedString("Message_TrackStatisticsFolder");
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    // create the root directory
                    try
                    {
                        OutputFolder = dialog.SelectedPath;
                    }
                    catch (Exception)
                    {
                        TrackStatistics = false;
                    }
                }
                else
                {
                    TrackStatistics = false;
                }
            }
        }
    }

    public SimulationOptions SimulationOptions
    {
        get => _simulationOptions;
        set
        {
            _simulationOptions = value;

            _absorptionWeightingTypeVM.Options[_simulationOptions.AbsorptionWeightingType].IsSelected = true;
            _randomNumberGeneratorTypeVM.Options[_simulationOptions.RandomNumberGeneratorType].IsSelected = true;
            _phaseFunctionTypeVM.Options[_simulationOptions.PhaseFunctionType].IsSelected = true;

            // note: the alternative to these below is to have SimulationOptions implement INotifyPropertyChanged (derive from BindableObject)
            OnPropertyChanged(nameof(Seed));
            //OnPropertyChanged("TallySecondMoment"); //This was moved to each detector input
            OnPropertyChanged(nameof(TrackStatistics));
            OnPropertyChanged(nameof(SimulationIndex));
        }
    }

    public int Seed
    {
        get => _simulationOptions.Seed;
        set
        {
            _simulationOptions.Seed = value;
            OnPropertyChanged(nameof(Seed));
        }
    }

    public bool TrackStatistics
    {
        get => _simulationOptions.TrackStatistics;
        set
        {
            _simulationOptions.TrackStatistics = value;
            OnPropertyChanged(nameof(TrackStatistics));
        }
    }

    public string OutputFolder
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(OutputFolder));
        }
    }

    public int SimulationIndex
    {
        get => _simulationOptions.SimulationIndex;
        set
        {
            _simulationOptions.SimulationIndex = value;
            OnPropertyChanged(nameof(SimulationIndex));
        }
    }

    public OptionViewModel<AbsorptionWeightingType> AbsorptionWeightingTypeVM
    {
        get => _absorptionWeightingTypeVM;
        set
        {
            _absorptionWeightingTypeVM = value;
            OnPropertyChanged(nameof(AbsorptionWeightingTypeVM));
        }
    }

    public OptionViewModel<RandomNumberGeneratorType> RandomNumberGeneratorTypeVM
    {
        get => _randomNumberGeneratorTypeVM;
        set
        {
            _randomNumberGeneratorTypeVM = value;
            OnPropertyChanged(nameof(RandomNumberGeneratorTypeVM));
        }
    }

    public OptionViewModel<PhaseFunctionType> PhaseFunctionTypeVM
    {
        get => _phaseFunctionTypeVM;
        set
        {
            _phaseFunctionTypeVM = value;
            OnPropertyChanged(nameof(PhaseFunctionTypeVM));
        }
    }
}
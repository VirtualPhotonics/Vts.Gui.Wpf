using System;
using System.Linq;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing domain sub-panel functionality (abstract - implemented for reflectance and fluence)
/// </summary>
public class AbstractSolutionDomainOptionViewModel<TDomainType> : OptionViewModel<TDomainType>
{
    private bool _allowMultiAxis;

    private bool _showIndependentAxisChoice;

    private bool _useSpectralInputs;

    public AbstractSolutionDomainOptionViewModel(string groupName, TDomainType defaultType)
        : base(groupName)
    {
        _useSpectralInputs = false;
        _allowMultiAxis = false;
        _showIndependentAxisChoice = false;
    }

    public AbstractSolutionDomainOptionViewModel()
        : this("", default(TDomainType))
    {
    }

    public OptionViewModel<IndependentVariableAxis> IndependentVariableAxisOptionVM
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IndependentVariableAxisOptionVM));
        }
    }

    public IndependentAxisViewModel[] IndependentAxesVMs
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IndependentAxesVMs));
        }
    }

    public ConstantAxisViewModel[] ConstantAxesVMs
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ConstantAxesVMs));
        }
    }

    public double ConstantAxisValueImageHeight
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ConstantAxisValueImageHeight));
        }
    }

    public double ConstantAxisValueTwoImageHeight
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ConstantAxisValueTwoImageHeight));
        }
    }

    public bool UseSpectralInputs
    {
        get => _useSpectralInputs;
        set
        {
            _useSpectralInputs = value;
            OnPropertyChanged(nameof(UseSpectralInputs));
        }
    }

    public bool AllowMultiAxis
    {
        get => _allowMultiAxis;
        set
        {
            _allowMultiAxis = value;
            OnPropertyChanged(nameof(AllowMultiAxis));
        }
    }

    public bool ShowIndependentAxisChoice
    {
        get => _showIndependentAxisChoice;
        set
        {
            _showIndependentAxisChoice = value;
            OnPropertyChanged(nameof(ShowIndependentAxisChoice));
        }
    }

    public virtual int NativeAxesCount => 1;

    public event EventHandler SettingsLoaded = delegate { };

    protected virtual void UpdateAxes()
    {
        var numAxes = IndependentVariableAxisOptionVM.SelectedValues.Length;
        var numConstants = IndependentVariableAxisOptionVM.UnSelectedValues.Length;

        // create new local VMs for independent and constant axes
        var independentAxesVMs = Enumerable.Range(0, numAxes).Select(i =>
            new IndependentAxisViewModel
            {
                AxisType = IndependentVariableAxisOptionVM.SelectedValues[i],
                AxisLabel = IndependentVariableAxisOptionVM.SelectedDisplayNames[i],
                AxisUnits = IndependentVariableAxisOptionVM.SelectedValues[i].GetUnits(),
                AxisRangeVM = new RangeViewModel(
                    IndependentVariableAxisOptionVM.SelectedValues[i].GetDefaultRange(),
                    IndependentVariableAxisOptionVM.SelectedValues[i].GetUnits(),
                    IndependentVariableAxisOptionVM.SelectedValues[i],
                    IndependentVariableAxisOptionVM.SelectedValues[i].GetTitle())
            }).ToArray();
        var constantAxesVMs = Enumerable.Range(0, numConstants).Select(i =>
            new ConstantAxisViewModel
            {
                AxisType = IndependentVariableAxisOptionVM.UnSelectedValues[i],
                AxisLabel = IndependentVariableAxisOptionVM.UnSelectedDisplayNames[i],
                AxisUnits = IndependentVariableAxisOptionVM.UnSelectedValues[i].GetUnits(),
                AxisValue = IndependentVariableAxisOptionVM.UnSelectedValues[i].GetDefaultConstantAxisValue()
            }).ToArray();

        // assign callbacks
        independentAxesVMs.ForEach(vm => vm.PropertyChanged += (s, a) => OnPropertyChanged(nameof(IndependentAxesVMs)));
        constantAxesVMs.ForEach(vm => vm.PropertyChanged += (s, a) => OnPropertyChanged(nameof(ConstantAxesVMs)));

        // and then set them, fully formed, to the member variable, firing change notification
        IndependentAxesVMs = independentAxesVMs;
        ConstantAxesVMs = constantAxesVMs;

        ShowIndependentAxisChoice = numAxes + numConstants > NativeAxesCount;
    }
}
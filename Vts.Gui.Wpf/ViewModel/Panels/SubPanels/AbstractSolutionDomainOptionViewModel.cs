using System.Linq;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel.Controls;

namespace Vts.Gui.Wpf.ViewModel.Panels.SubPanels;

/// <summary>
///     View model implementing domain sub-panel functionality (abstract - implemented for reflectance and fluence)
/// </summary>
public class AbstractSolutionDomainOptionViewModel<TDomainType>(string groupName) : OptionViewModel<TDomainType>(groupName)
{
    public AbstractSolutionDomainOptionViewModel()
        : this("")
    {
    }

    public OptionViewModel<IndependentVariableAxis> IndependentVariableAxisOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IndependentVariableAxisOptionVm));
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

    public bool UseSpectralInputs
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(UseSpectralInputs));
        }
    } = false;

    public bool AllowMultiAxis
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AllowMultiAxis));
        }
    } = false;

    public bool ShowIndependentAxisChoice
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ShowIndependentAxisChoice));
        }
    } = false;

    public virtual int NativeAxesCount => 1;

    protected virtual void UpdateAxes()
    {
        var numAxes = IndependentVariableAxisOptionVm.SelectedValues.Length;
        var numConstants = IndependentVariableAxisOptionVm.UnSelectedValues.Length;

        // create new local VMs for independent and constant axes
        var independentAxesVMs = Enumerable.Range(0, numAxes).Select(i =>
            new IndependentAxisViewModel
            {
                AxisType = IndependentVariableAxisOptionVm.SelectedValues[i],
                AxisLabel = IndependentVariableAxisOptionVm.SelectedDisplayNames[i],
                AxisUnits = IndependentVariableAxisOptionVm.SelectedValues[i].GetUnits(),
                AxisRangeVm = new RangeViewModel(
                    IndependentVariableAxisOptionVm.SelectedValues[i].GetDefaultRange(),
                    IndependentVariableAxisOptionVm.SelectedValues[i].GetUnits(),
                    IndependentVariableAxisOptionVm.SelectedValues[i],
                    IndependentVariableAxisOptionVm.SelectedValues[i].GetTitle())
            }).ToArray();
        var constantAxesVMs = Enumerable.Range(0, numConstants).Select(i =>
            new ConstantAxisViewModel
            {
                AxisType = IndependentVariableAxisOptionVm.UnSelectedValues[i],
                AxisLabel = IndependentVariableAxisOptionVm.UnSelectedDisplayNames[i],
                AxisUnits = IndependentVariableAxisOptionVm.UnSelectedValues[i].GetUnits(),
                AxisValue = IndependentVariableAxisOptionVm.UnSelectedValues[i].GetDefaultConstantAxisValue()
            }).ToArray();

        // assign callbacks
        independentAxesVMs.ForEach(vm => vm.PropertyChanged += (_, _) => OnPropertyChanged(nameof(IndependentAxesVMs)));
        constantAxesVMs.ForEach(vm => vm.PropertyChanged += (_, _) => OnPropertyChanged(nameof(ConstantAxesVMs)));

        // and then set them, fully formed, to the member variable, firing change notification
        IndependentAxesVMs = independentAxesVMs;
        ConstantAxesVMs = constantAxesVMs;

        ShowIndependentAxisChoice = numAxes + numConstants > NativeAxesCount;
    }
}
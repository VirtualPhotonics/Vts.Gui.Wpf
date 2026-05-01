using System;
using System.ComponentModel;
using System.Linq;
using Vts.Gui.Wpf.Helpers;
using Vts.Gui.Wpf.ViewModel.Controls;
using Vts.Gui.Wpf.ViewModel.Panels.SubPanels;

namespace Vts.Gui.Wpf.ViewModel.Panels;

/// <summary>
/// Base view model for solvers that provides common properties and logic for handling solution domain options,
/// including the use of spectral panel data and the display of optical properties.
/// </summary>
public abstract class BaseSolverViewModel : BindableObject
{
    private RangeViewModel[] _allRangeVMs;

    public RangeViewModel[] AllRangeVMs
    {
        get => _allRangeVMs;
        set
        {
            _allRangeVMs = value;
            OnPropertyChanged(nameof(AllRangeVMs));
        }
    }

    public bool ShowOpticalProperties
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ShowOpticalProperties));
        }
    }

    public bool UseSpectralPanelData
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(UseSpectralPanelData));
        }
    }

    public SolutionDomainOptionViewModel SolutionDomainTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(SolutionDomainTypeOptionVm));
        }
    }

    /// <summary>
    /// Handles the <see cref="BindableObject.PropertyChanged"/> event for an optical property view model.
    /// Updates the <see cref="UseSpectralPanelData"/> property based on the current state of 
    /// <see cref="SolutionDomainTypeOptionVm"/> and updates the wavelength with the value from
    /// the spectral panel if spectral panel data is being used.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The property that changed.</param>
    /// <remarks>The optical properties will be updated by the spectral panel if the
    /// "use spectral panel inputs" checkbox is checked.</remarks>
    protected void OpticalPropertyVm_PropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (UseSpectralPanelData && WindowViewModel.Current != null &&
            WindowViewModel.Current.SpectralMappingVm != null)
        {
            UpdateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVm.Wavelength);
        }
    }

    /// <summary>
    /// Handles the <see cref="BindableObject.PropertyChanged"/> event for the <c>SolutionDomainTypeOptionVm</c>.
    /// Updates dependent properties or performs actions based on the property that changed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the <c>SolutionDomainTypeOptionVm</c>.</param>
    /// <param name="args">The property that changed.</param>
    protected void SolutionDomainTypeOptionVm_PropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        var useSpectralPanelDataAndNotNull = SolutionDomainTypeOptionVm.UseSpectralInputs &&
                                             WindowViewModel.Current != null &&
                                             WindowViewModel.Current.SpectralMappingVm != null;

        switch (args.PropertyName)
        {
            case "ConstantAxesVMs":
                if (useSpectralPanelDataAndNotNull && WindowViewModel.Current != null &&
                    SolutionDomainTypeOptionVm.ConstantAxesVMs.Any(
                        axis => axis.AxisType == IndependentVariableAxis.Wavelength))
                {
                    WindowViewModel.Current.SpectralMappingVm.Wavelength = SolutionDomainTypeOptionVm.ConstantAxesVMs
                        .First(axis => axis.AxisType == IndependentVariableAxis.Wavelength).AxisValue;
                }
                break;
            case "UseSpectralInputs":
                UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
                break;
            case "IndependentAxesVMs":
                {
                    AllRangeVMs =
                        (from i in
                                Enumerable.Range(0,
                                    SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.Length)
                         orderby i descending
                         select
                         useSpectralPanelDataAndNotNull &&
                         SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues[i] ==
                         IndependentVariableAxis.Wavelength
                             ? WindowViewModel.Current.SpectralMappingVm.WavelengthRangeVm
                             : SolutionDomainTypeOptionVm.IndependentAxesVMs[i].AxisRangeVm).ToArray();

                    ShowOpticalProperties = _allRangeVMs.All(value => value.AxisType != IndependentVariableAxis.Wavelength);

                    if (useSpectralPanelDataAndNotNull &&
                        SolutionDomainTypeOptionVm.ConstantAxesVMs.Any(
                            axis => axis.AxisType == IndependentVariableAxis.Wavelength) &&
                        WindowViewModel.Current != null &&
                        WindowViewModel.Current.SpectralMappingVm != null)
                    {
                        UpdateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVm.Wavelength);
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Updates the solution domain's wavelength axis value if it differs from the specified wavelength.
    /// </summary>
    /// <remarks>If the solution domain does not contain a wavelength axis, or if the current value is already
    /// approximately equal to the specified wavelength, no update is performed.</remarks>
    /// <param name="wv">The wavelength value to set for the solution domain's wavelength axis.</param>
    protected void UpdateSolutionDomainWithWavelength(double wv)
    {
        var wvAxis = SolutionDomainTypeOptionVm.ConstantAxesVMs.FirstOrDefault(axis => axis.AxisType == IndependentVariableAxis.Wavelength);
        if (wvAxis != null && !Calculations.AreApproximatelyEqual(wvAxis.AxisValue, wv))
        {
            wvAxis.AxisValue = wv;
        }
    }

    /// <summary>
    /// Function to provide ordering information for assembling forward calls
    /// </summary>
    /// <param name="axis">The independent variable axis for which to determine the order.</param>
    /// <returns>The order value for the specified axis.</returns>
    protected static int GetParameterOrder(IndependentVariableAxis axis)
    {
        return axis switch
        {
            IndependentVariableAxis.Wavelength => 0,
            IndependentVariableAxis.Rho => 1,
            IndependentVariableAxis.Fx => 1,
            IndependentVariableAxis.Time => 2,
            IndependentVariableAxis.Ft => 2,
            IndependentVariableAxis.Z => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(axis))
        };
    }
}
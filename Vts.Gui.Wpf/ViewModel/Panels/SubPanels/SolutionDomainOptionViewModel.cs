using System;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing Reflectance domain sub-panel functionality
/// </summary>
public class SolutionDomainOptionViewModel : AbstractSolutionDomainOptionViewModel<SolutionDomainType>
{
    private bool _enableMultiAxis;      // local property saving inherited AllowMultiAxis state
    private bool _enableSpectralPanelInputs; // local property saving inherited UseSpectralInputs state


    public SolutionDomainOptionViewModel(string groupName, SolutionDomainType defaultType)
        : base(groupName, defaultType)
    {
        _enableMultiAxis = true;
        _enableSpectralPanelInputs = true;
        ROfRhoOption = Options[SolutionDomainType.ROfRho];
        ROfFxOption = Options[SolutionDomainType.ROfFx];
        ROfRhoAndTimeOption = Options[SolutionDomainType.ROfRhoAndTime];
        ROfFxAndTimeOption = Options[SolutionDomainType.ROfFxAndTime];
        ROfRhoAndFtOption = Options[SolutionDomainType.ROfRhoAndFt];
        ROfFxAndFtOption = Options[SolutionDomainType.ROfFxAndFt];

        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "SelectedValue" ||
                args.PropertyName == "UseSpectralInputs" ||
                args.PropertyName == "AllowMultiAxis")
                UpdateOptions();
        };
        SelectedValue = defaultType;
        UpdateOptions();
    }

    public SolutionDomainOptionViewModel()
        : this("", SolutionDomainType.ROfRho)
    {
    }

    public OptionModel<SolutionDomainType> ROfRhoOption { get; }
    public OptionModel<SolutionDomainType> ROfFxOption { get; private set; }
    public OptionModel<SolutionDomainType> ROfRhoAndTimeOption { get; }
    public OptionModel<SolutionDomainType> ROfFxAndTimeOption { get; }
    public OptionModel<SolutionDomainType> ROfRhoAndFtOption { get; }
    public OptionModel<SolutionDomainType> ROfFxAndFtOption { get; }

    public bool EnableMultiAxis
    {
        get => _enableMultiAxis;

        set
        {
            _enableMultiAxis = value;
            OnPropertyChanged(nameof(EnableMultiAxis));
        }
    }

    public bool EnableSpectralPanelInputs
    {
        get => _enableSpectralPanelInputs;

        set
        {
            _enableSpectralPanelInputs = value;
            OnPropertyChanged(nameof(EnableSpectralPanelInputs));
        }
    }

    public bool IsROfRhoAndTimeEnabled
    {
        get;
        set
        {
            field = value;
            ROfRhoAndTimeOption.IsEnabled = value;
            OnPropertyChanged(nameof(ROfRhoAndTimeOption));
        }
    }

    public bool IsROfRhoAndFtEnabled
    {
        get;
        set
        {
            field = value;
            ROfRhoAndFtOption.IsEnabled = value;
            OnPropertyChanged(nameof(ROfRhoAndFtOption));
        }
    }

    public bool IsROfFxAndTimeEnabled
    {
        get;
        set
        {
            field = value;
            ROfFxAndTimeOption.IsEnabled = value;
            OnPropertyChanged(nameof(ROfFxAndTimeOption));
        }
    }

    public bool IsROfFxAndFtEnabled
    {
        get;
        set
        {
            field = value;
            ROfFxAndFtOption.IsEnabled = value;
            OnPropertyChanged(nameof(ROfFxAndFtOption));
        }
    }

    private void UpdateOptions()
    {
        switch (SelectedValue)
        {
            case SolutionDomainType.ROfRho:
                IndependentVariableAxisOptionVM = UseSpectralInputs
                    ? new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Rho,
                        new[] {IndependentVariableAxis.Rho, IndependentVariableAxis.Wavelength}, AllowMultiAxis)
                    : new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Rho, new[] {IndependentVariableAxis.Rho}, AllowMultiAxis);
                ROfRhoOption.IsSelected = true;
                break;
            case SolutionDomainType.ROfFx:
                IndependentVariableAxisOptionVM = UseSpectralInputs
                    ? new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Fx,
                        new[] {IndependentVariableAxis.Fx, IndependentVariableAxis.Wavelength}, AllowMultiAxis)
                    : new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Fx, new[] {IndependentVariableAxis.Fx}, AllowMultiAxis);
                break;
            case SolutionDomainType.ROfRhoAndTime:
                IndependentVariableAxisOptionVM = UseSpectralInputs
                    ? new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Rho,
                        new[]
                        {
                            IndependentVariableAxis.Rho, IndependentVariableAxis.Time,
                            IndependentVariableAxis.Wavelength
                        }, AllowMultiAxis)
                    : new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Rho,
                        new[] {IndependentVariableAxis.Rho, IndependentVariableAxis.Time}, AllowMultiAxis);
                break;
            case SolutionDomainType.ROfFxAndTime:
                IndependentVariableAxisOptionVM = UseSpectralInputs
                    ? new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Fx,
                        new[]
                        {
                            IndependentVariableAxis.Fx, IndependentVariableAxis.Time,
                            IndependentVariableAxis.Wavelength
                        }, AllowMultiAxis)
                    : new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Fx, new[] {IndependentVariableAxis.Fx, IndependentVariableAxis.Time},
                        AllowMultiAxis);
                break;
            case SolutionDomainType.ROfRhoAndFt:
                IndependentVariableAxisOptionVM = UseSpectralInputs
                    ? new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Rho,
                        new[]
                        {
                            IndependentVariableAxis.Rho, IndependentVariableAxis.Ft, IndependentVariableAxis.Wavelength
                        }, AllowMultiAxis)
                    : new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Rho, new[] {IndependentVariableAxis.Rho, IndependentVariableAxis.Ft},
                        AllowMultiAxis);
                break;
            case SolutionDomainType.ROfFxAndFt:
                IndependentVariableAxisOptionVM = UseSpectralInputs
                    ? new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Fx,
                        new[]
                        {IndependentVariableAxis.Fx, IndependentVariableAxis.Ft, IndependentVariableAxis.Wavelength},
                        AllowMultiAxis)
                    : new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                        IndependentVariableAxis.Fx, new[] {IndependentVariableAxis.Fx, IndependentVariableAxis.Ft},
                        AllowMultiAxis);
                break;
            default:
                throw new NotImplementedException("selectedType");
        }

        // create a new callback based on the new viewmodel
        IndependentVariableAxisOptionVM.PropertyChanged += (s, a) => UpdateAxes();

        UpdateAxes();
    }
}
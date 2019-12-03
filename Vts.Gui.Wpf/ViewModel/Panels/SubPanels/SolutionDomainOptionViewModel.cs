using System;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing Reflectance domain sub-panel functionality
    /// </summary>
    public class SolutionDomainOptionViewModel : AbstractSolutionDomainOptionViewModel<SolutionDomainType>
    {
        private bool _enableMultiAxis;      // local property saving inherited AllowMultiAxis state
        private bool _enableSpectralPanelInputs; // local property saving inherited UseSpectralInputs state
        private bool _isROfRhoAndTimeEnabled;
        private bool _isROfRhoAndFtEnabled;
        private bool _isROfFxAndTimeEnabled;
        private bool _isROfFxAndFtEnabled;


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

        public OptionModel<SolutionDomainType> ROfRhoOption { get; private set; }
        public OptionModel<SolutionDomainType> ROfFxOption { get; private set; }
        public OptionModel<SolutionDomainType> ROfRhoAndTimeOption { get; private set; }
        public OptionModel<SolutionDomainType> ROfFxAndTimeOption { get; private set; }
        public OptionModel<SolutionDomainType> ROfRhoAndFtOption { get; private set; }
        public OptionModel<SolutionDomainType> ROfFxAndFtOption { get; private set; }

        public bool EnableMultiAxis
        {
            get { return _enableMultiAxis; }

            set
            {
                _enableMultiAxis = value;
                OnPropertyChanged("EnableMultiAxis");
            }
        }

        public bool EnableSpectralPanelInputs
        {
            get { return _enableSpectralPanelInputs; }

            set
            {
                _enableSpectralPanelInputs = value;
                OnPropertyChanged("EnableSpectralPanelInputs");
            }
        }
        public bool IsROfRhoAndTimeEnabled
        {
            get { return _isROfRhoAndTimeEnabled; }
            set
            {
                _isROfRhoAndTimeEnabled = value;
                ROfRhoAndTimeOption.IsEnabled = value;
                OnPropertyChanged("ROfRhoAndTimeOption");
            }
        }
        public bool IsROfRhoAndFtEnabled
        {
            get { return _isROfRhoAndFtEnabled; }
            set
            {
                _isROfRhoAndFtEnabled = value;
                ROfRhoAndFtOption.IsEnabled = value;
                OnPropertyChanged("ROfRhoAndFtOption");
            }
        }
        public bool IsROfFxAndTimeEnabled
        {
            get { return _isROfFxAndTimeEnabled; }
            set
            {
                _isROfFxAndTimeEnabled = value;
                ROfFxAndTimeOption.IsEnabled = value;
                OnPropertyChanged("ROfFxAndTimeOption");
            }
        }
        public bool IsROfFxAndFtEnabled
        {
            get { return _isROfFxAndFtEnabled; }
            set
            {
                _isROfFxAndFtEnabled = value;
                ROfFxAndFtOption.IsEnabled = value;
                OnPropertyChanged("ROfFxAndFtOption");
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
}
using System;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing Fluence domain sub-panel functionality
    /// </summary>
    public class FluenceSolutionDomainOptionViewModel : AbstractSolutionDomainOptionViewModel<FluenceSolutionDomainType>
    {
        private bool _isFluenceOfRhoAndZAndFtEnabled;

        private bool _isFluenceOfRhoAndZAndTimeEnabled;

        public FluenceSolutionDomainOptionViewModel(string groupName, FluenceSolutionDomainType defaultType)
            : base(groupName, defaultType)
        {
            //InitializeControls
            FluenceOfRhoAndZOption = Options[FluenceSolutionDomainType.FluenceOfRhoAndZ];
            FluenceOfFxAndZOption = Options[FluenceSolutionDomainType.FluenceOfFxAndZ];
            FluenceOfRhoAndZAndTimeOption = Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndTime];
            FluenceOfFxAndZAndTimeOption = Options[FluenceSolutionDomainType.FluenceOfFxAndZAndTime];
            FluenceOfRhoAndZAndFtOption = Options[FluenceSolutionDomainType.FluenceOfRhoAndZAndFt];
            FluenceOfFxAndZAndFtOption = Options[FluenceSolutionDomainType.FluenceOfFxAndZAndFt];

            PropertyChanged += (sender, args) =>
            {
                if (sender is FluenceSolutionDomainOptionViewModel && args.PropertyName == "SelectedValue")
                {
                    UpdateOptions();
                }
            };
            UpdateOptions();
        }

        public FluenceSolutionDomainOptionViewModel()
            : this("", FluenceSolutionDomainType.FluenceOfRhoAndZ)
        {
        }

        public OptionModel<FluenceSolutionDomainType> FluenceOfRhoAndZOption { get; private set; }
        public OptionModel<FluenceSolutionDomainType> FluenceOfFxAndZOption { get; private set; }
        public OptionModel<FluenceSolutionDomainType> FluenceOfRhoAndZAndTimeOption { get; private set; }
        public OptionModel<FluenceSolutionDomainType> FluenceOfFxAndZAndTimeOption { get; private set; }
        public OptionModel<FluenceSolutionDomainType> FluenceOfRhoAndZAndFtOption { get; private set; }
        public OptionModel<FluenceSolutionDomainType> FluenceOfFxAndZAndFtOption { get; private set; }

        public bool IsFluenceOfRhoAndZAndTimeEnabled
        {
            get => _isFluenceOfRhoAndZAndTimeEnabled;
            set
            {
                _isFluenceOfRhoAndZAndTimeEnabled = value;
                FluenceOfRhoAndZAndTimeOption.IsEnabled = value;
                OnPropertyChanged("FluenceOfRhoAndZAndTimeOption");
            }
        }

        public bool IsFluenceOfRhoAndZAndFtEnabled
        {
            get => _isFluenceOfRhoAndZAndFtEnabled;
            set
            {
                _isFluenceOfRhoAndZAndFtEnabled = value;
                FluenceOfRhoAndZAndFtOption.IsEnabled = value;
                OnPropertyChanged("FluenceOfRhoAndZAndFtOption");
            }
        }

        public override int NativeAxesCount => 1;

        private void UpdateOptions()
        {
            switch (SelectedValue)
            {
                case FluenceSolutionDomainType.FluenceOfRhoAndZ:
                    IndependentVariableAxisOptionVM =
                        new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                            new[] {IndependentVariableAxis.Rho});
                    FluenceOfRhoAndZOption.IsSelected = true;
                        //added to force the radio button when it is changed programatically
                    break;
                case FluenceSolutionDomainType.FluenceOfFxAndZ:
                    IndependentVariableAxisOptionVM =
                        new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                            new[] {IndependentVariableAxis.Fx});
                    break;
                case FluenceSolutionDomainType.FluenceOfRhoAndZAndTime:
                    IndependentVariableAxisOptionVM =
                        new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                            new[] {IndependentVariableAxis.Rho, IndependentVariableAxis.Time});
                    break;
                case FluenceSolutionDomainType.FluenceOfFxAndZAndTime:
                    IndependentVariableAxisOptionVM =
                        new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                            new[] {IndependentVariableAxis.Fx, IndependentVariableAxis.Time});
                    break;
                case FluenceSolutionDomainType.FluenceOfRhoAndZAndFt:
                    IndependentVariableAxisOptionVM =
                        new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                            new[] {IndependentVariableAxis.Rho, IndependentVariableAxis.Ft});
                    break;
                case FluenceSolutionDomainType.FluenceOfFxAndZAndFt:
                    IndependentVariableAxisOptionVM =
                        new OptionViewModel<IndependentVariableAxis>("IndependentAxis", false,
                            new[] {IndependentVariableAxis.Fx, IndependentVariableAxis.Ft});
                    break;
                default:
                    throw new NotImplementedException("SelectedValue");
            }

            // create a new callback based on the new viewmodel
            IndependentVariableAxisOptionVM.PropertyChanged += (s, a) => UpdateAxes();

            UpdateAxes();
            //The independent axis should not be visible, this panel already has modulation frequency
            ShowIndependentAxisChoice = ShowIndependentAxisChoice &&
                                        (SelectedValue != FluenceSolutionDomainType.FluenceOfRhoAndZAndFt) &&
                                        (SelectedValue != FluenceSolutionDomainType.FluenceOfRhoAndZAndTime);
        }
    }
}
using System;
using System.Globalization;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels;

/// <summary>
/// Summary description for InverseSolverViewModelTests
/// </summary>
[TestFixture]
public class InverseSolverViewModelTests
{
    /// <summary>
    /// Verifies that InverseSolverViewModel default constructor instantiates sub view models
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.InverseSolverVm;
        Assert.That(viewModel.SolutionDomainTypeOptionVm, Is.InstanceOf<SolutionDomainOptionViewModel>());
        Assert.That(viewModel.MeasuredForwardSolverTypeOptionVm, Is.InstanceOf<OptionViewModel<ForwardSolverType>>());
        Assert.That(viewModel.InverseForwardSolverTypeOptionVm, Is.InstanceOf<OptionViewModel<ForwardSolverType>>());
        Assert.That(viewModel.InverseFitTypeOptionVm, Is.InstanceOf<OptionViewModel<InverseFitType>>());
        Assert.That(viewModel.OptimizerTypeOptionVm, Is.InstanceOf<OptionViewModel<OptimizerType>>());
        Assert.That(viewModel.MeasuredOpticalPropertyVm, Is.InstanceOf<OpticalPropertyViewModel>());
        Assert.That(viewModel.InitialGuessOpticalPropertyVm, Is.InstanceOf<OpticalPropertyViewModel>());
        Assert.That(viewModel.ResultOpticalPropertyVm, Is.InstanceOf<OpticalPropertyViewModel>());
        Assert.That(viewModel.AllRangeVMs, Is.InstanceOf<RangeViewModel[]>());
        Assert.That(viewModel.SolutionDomainTypeOptionVm.EnableMultiAxis, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.AllowMultiAxis, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.SelectedValue, Is.EqualTo(SolutionDomainType.ROfRho));
        Assert.That(viewModel.AllRangeVMs.Length, Is.EqualTo(1));
        Assert.That(Math.Abs(viewModel.AllRangeVMs[0].Start - 0.5) < 1e-6, Is.True);
        Assert.That(Math.Abs(viewModel.AllRangeVMs[0].Stop - 9.5) < 1e-6, Is.True);
        Assert.That(viewModel.AllRangeVMs[0].Number, Is.EqualTo(19));
    }

    // The following tests verify the Relay Commands
    /// <summary>
    /// Verifies that all commands, SimulateMeasuredCommand, CalculateInitialGuessCommand,
    /// and SolveInverseCommand returns correct values
    /// </summary>
    [Test]
    public void Verify_SimulateMeasuredCommand_returns_correct_values()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.InverseSolverVm;
        viewModel.MeasuredForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.Nurbs;
        viewModel.InverseForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.PointSourceSDA;
        viewModel.SolutionDomainTypeOptionVm.SelectedValue = SolutionDomainType.ROfRho;
        viewModel.AllRangeVMs[0].Start = 1.0;
        viewModel.AllRangeVMs[0].Stop = 10.0;
        viewModel.AllRangeVMs[0].Number = 10;
        viewModel.PercentNoise = 0;
        // SimulateMeasuredDataCommand
        viewModel.SimulateMeasuredDataCommand.Execute(null);
        var plotViewModel = windowViewModel.PlotVm;
        const double d1 = 0.01;
        const int i1 = 1;
        const double g = 0.8;
        const double n = 1.4;
        const double d2 = 0.0100;
        const double d3 = 1.0000;
        const double d4 = 0.0129;
        const double d5 = 0.9255;
        const double muaError = 28.9;
        const double muspError = 7.45;
        var op1 = StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d1.ToString(CultureInfo.CurrentCulture) + " " +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 i1.ToString(CultureInfo.CurrentCulture) + " g=" +
                 g.ToString(CultureInfo.CurrentCulture) + " n=" +
                 n.ToString(CultureInfo.CurrentCulture) + "; " +
                 StringLookup.GetLocalizedString("Label_Units") + " = 1/mm \r";
        var op2 = StringLookup.GetLocalizedString("Label_MuA") + "=" +
                  d4.ToString(CultureInfo.CurrentCulture) + " " +
                  StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                  d5.ToString(CultureInfo.CurrentCulture) + " g=" +
                  g.ToString(CultureInfo.CurrentCulture) + " n=" +
                  n.ToString(CultureInfo.CurrentCulture) + "; " +
                  StringLookup.GetLocalizedString("Label_Units") + " = 1/mm \r";
        var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [mm-2] " +
                 StringLookup.GetLocalizedString("Label_Versus") + " ρ [mm]";
        var s3 = "\n" + StringLookup.GetLocalizedString("Label_Simulated") + "\r" +
                 StringLookup.GetLocalizedString("Label_ModelNurbs") + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 d3.ToString("N4", CultureInfo.CurrentCulture);
        var s4 = "\n" + StringLookup.GetLocalizedString("Label_Guess") + "\r" +
                 StringLookup.GetLocalizedString("Label_ModelSDA") + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 d3.ToString("N4", CultureInfo.CurrentCulture);
        var s5 = "\n" + StringLookup.GetLocalizedString("Label_Calculated") + "\r" +
                 StringLookup.GetLocalizedString("Label_ModelNurbs") + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d4.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 d5.ToString("N4", CultureInfo.CurrentCulture);
        var s6 = StringLookup.GetLocalizedString("Label_SimulatedMeasuredData") + op1;
        var s7 = StringLookup.GetLocalizedString("Label_InitialGuess") + op1;
        var s8 = StringLookup.GetLocalizedString("Label_InverseSolutionResults") + "\r   " +
                 StringLookup.GetLocalizedString("Label_OptimizationParameter") + "MuaMusp \r   " +
                 s7 + "   " + StringLookup.GetLocalizedString("Label_Exact") + ": " + op1 +
                 "   " + StringLookup.GetLocalizedString("Label_ConvergedValues") + ": " + op2 +
                 "   " + StringLookup.GetLocalizedString("Label_PercentError") + 
                 StringLookup.GetLocalizedString("Label_MuA") + " = " + 
                 muaError.ToString(CultureInfo.CurrentCulture) + "%  " + 
                 StringLookup.GetLocalizedString("Label_MuSPrime") + " = " + 
                 muspError.ToString(CultureInfo.CurrentCulture) + "% \r";
        Assert.That(plotViewModel.Labels[0], Is.EqualTo(s3));
        Assert.That(plotViewModel.Title, Is.EqualTo(s2));
        var textOutputViewModel = windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s6));
        // CalculateInitialGuessCommand
        viewModel.CalculateInitialGuessCommand.Execute(null);
        Assert.That(plotViewModel.Labels[1], Is.EqualTo(s4));
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s6 + s7));
        // SolveInverseCommand
        viewModel.SolveInverseCommand.Execute(null);
        Assert.That(plotViewModel.Labels[2], Is.EqualTo(s5));
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s6 + s7 + s8));
    }
}

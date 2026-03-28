using System.Globalization;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels;

/// <summary>
/// Summary description for ForwardSolverViewModelTests
/// </summary>
[TestFixture]
public class ForwardSolverViewModelTests
{
    /// <summary>
    /// Verifies that ForwardSolverViewModel default constructor instantiates sub view models
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.ForwardSolverVm;
        Assert.That(viewModel.ForwardSolverTypeOptionVm != null, Is.True);
        Assert.That(viewModel.SolutionDomainTypeOptionVm != null, Is.True);
        Assert.That(viewModel.ForwardAnalysisTypeOptionVm != null, Is.True);
    }

    // The following tests verify the Relay Commands
    /// <summary>
    /// Verifies that the ExecuteForwardSolverCommand returns correct values
    /// </summary>
    [Test]
    public void Verify_ExecuteForwardSolverCommand_returns_correct_values()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.ForwardSolverVm;
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.PointSourceSDA;
        viewModel.SolutionDomainTypeOptionVm.SelectedValue = SolutionDomainType.ROfRho;
        viewModel.ExecuteForwardSolverCommand.Execute(null);
        // ExecuteForwardSolver default settings
        var plotViewModel = windowViewModel.PlotVm;
        const double d1 = 0.01;
        const int i1 = 1;
        const double g = 0.8;
        const double n = 1.4;
        const double d2 = 0.0100;
        const double d3 = 1.0000;
        var s1 = StringLookup.GetLocalizedString("Label_ForwardSolver") +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d1.ToString(CultureInfo.CurrentCulture) + " " +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 i1.ToString(CultureInfo.CurrentCulture) + " g=" +
                 g.ToString(CultureInfo.CurrentCulture) + " n=" +
                 n.ToString(CultureInfo.CurrentCulture) + "; " +
                 StringLookup.GetLocalizedString("Label_Units") + " = 1/mm\r";
        var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [mm-2] " +
                 StringLookup.GetLocalizedString("Label_Versus") + " ρ [mm]";
        var s3 = "\r" + StringLookup.GetLocalizedString("Label_ModelSDA") + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 d3.ToString("N4", CultureInfo.CurrentCulture);
        Assert.That(plotViewModel.Labels[0], Is.EqualTo(s3));
        Assert.That(plotViewModel.Title, Is.EqualTo(s2));
        var textOutputViewModel = windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s1));
    }

    /// <summary>
    /// Verifies that ForwardSolverViewModel returns correct multi-region tissue values
    /// </summary>
    [Test]
    public void Verify_ExecuteForwardSolverCommand_multi_region_tissue_returns_correct_values()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.ForwardSolverVm;
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.TwoLayerSDA;
        viewModel.SolutionDomainTypeOptionVm.SelectedValue = SolutionDomainType.ROfFx;
        viewModel.ForwardAnalysisTypeOptionVm.SelectedValue = ForwardAnalysisType.R;
        viewModel.ExecuteForwardSolverCommand.Execute(null); 
        var plotViewModel = windowViewModel.PlotVm;
        const int i1 = 1;
        const double d2 = 0.0100;
        // s1 should be "Plot View: plot cleared due to independent axis variable change
        // Forward Solver: Vts.Gui.Wpf.ViewModel.MultiRegionTissueViewModel"
        var s1 = StringLookup.GetLocalizedString("Message_PlotViewCleared") + "\r" +
                     StringLookup.GetLocalizedString("Label_ForwardSolver") +
                     "Vts.Gui.Wpf.ViewModel.MultiRegionTissueViewModel\r";
        // s2 should be "R(ρ) [Unitless] versus fx [1/mm]"
        var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [Unitless] " +
                 StringLookup.GetLocalizedString("Label_Versus") + " fx [1/mm]";
        // s3 should be "Model - 2 layer SDA\rμa1 = 0.0100\rμs'1=1.0000\rμa2 = 0.0100\r μs'2=1.0000"
        var s3 = "\r" + StringLookup.GetLocalizedString("Label_Model2LayerSDA") + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA1") + "=" +
                 d2.ToString("N4", CultureInfo.CurrentCulture) + "\r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime1") + "=" +
                 i1.ToString("N4", CultureInfo.CurrentCulture) + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA2") + "=" +
                 d2.ToString("N4", CultureInfo.CurrentCulture) + "\r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime2") + "=" +
                 i1.ToString("N4", CultureInfo.CurrentCulture);
        Assert.That(plotViewModel.Labels[0], Is.EqualTo(s3));
        Assert.That(plotViewModel.Title, Is.EqualTo(s2));
        var textOutputViewModel = windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s1));
    }

    /// <summary>
    /// Verifies that ForwardSolverViewModel returns correct complex derivative values
    /// </summary>
    [Test]
    public void Verify_ExecuteForwardSolverCommand_complex_derivative_returns_correct_values()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.ForwardSolverVm;
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.MonteCarlo;
        viewModel.SolutionDomainTypeOptionVm.SelectedValue = SolutionDomainType.ROfFxAndFt;
        viewModel.ForwardAnalysisTypeOptionVm.SelectedValue = ForwardAnalysisType.dRdMua;
        viewModel.ExecuteForwardSolverCommand.Execute(null);
        var plotViewModel = windowViewModel.PlotVm;
        const double d1 = 0.01;
        const int i1 = 1;
        const double g = 0.8;
        const double n = 1.4;
        const double d2 = 0.0100;
        const double d3 = 1.0000;
        const double d4 = 0;
        // s1 should be "Plot View: plot cleared due to independent axis variable change
        // Forward Solver: μa = 0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm"
        var s1 = StringLookup.GetLocalizedString("Message_PlotViewCleared") + "\r" +
                 StringLookup.GetLocalizedString("Label_ForwardSolver") +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d1.ToString(CultureInfo.CurrentCulture) + " " +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 i1.ToString(CultureInfo.CurrentCulture) + " g=" +
                 g.ToString(CultureInfo.CurrentCulture) + " n=" +
                 n.ToString(CultureInfo.CurrentCulture) + "; " +
                 StringLookup.GetLocalizedString("Label_Units") + " = 1/mm\r";
        // s2 should be "R(ρ) [GHz-1] versus fx [1/mm]"
        var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [GHz-1] " +
                 StringLookup.GetLocalizedString("Label_Versus") + " fx [1/mm]";
        // "ft" is not in Strings.resx
        var s3 = "\r" + StringLookup.GetLocalizedString("Label_ModelScaledMC") + "\r" +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 d3.ToString("N4", CultureInfo.CurrentCulture) + " \rft = " +
                 d4.ToString("N0", CultureInfo.CurrentCulture) + " " +
                 StringLookup.GetLocalizedString("Measurement_GHz");
        Assert.That(plotViewModel.Labels[0], Is.EqualTo(s3));
        Assert.That(plotViewModel.Title, Is.EqualTo(s2));
        var textOutputViewModel = windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s1));
    }

    /// <summary>
    /// Verifies that ForwardSolverViewModel disallows spectral panel inputs for TwoLayerSDA selection
    /// </summary>
    [Test]
    public void Verify_TwoLayerSDA_selection_does_not_display_UseSpectralInputs_checkbox()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.ForwardSolverVm;
        // set forward solver to TwoLayerSDA
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.TwoLayerSDA;
        Assert.That(viewModel.SolutionDomainTypeOptionVm.AllowMultiAxis, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.UseSpectralInputs, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.EnableMultiAxis, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.EnableSpectralPanelInputs, Is.False);
    }

    /// <summary>
    /// Verifies that ForwardSolverViewModel disallows time-dependent solution domain options
    /// for DistributedGaussianSourceSDA selection
    /// </summary>
    [Test]
    public void Verify_DistributedGaussianSourceSDA_selection_does_not_display_time_dependent_solution_domain_options()
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var viewModel = windowViewModel.ForwardSolverVm;
        // set forward solver to DistributedGaussianSourceSDA
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.DistributedGaussianSourceSDA;

        Assert.That(viewModel.SolutionDomainTypeOptionVm.IsROfRhoAndTimeEnabled, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.IsROfRhoAndFtEnabled, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.IsROfFxAndTimeEnabled, Is.False);
        Assert.That(viewModel.SolutionDomainTypeOptionVm.IsROfFxAndFtEnabled, Is.False);
    }


}

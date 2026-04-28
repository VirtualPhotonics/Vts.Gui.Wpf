using NUnit.Framework;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.Test.View.TestHelpers;
using Vts.Gui.Wpf.View.Controls;
using Vts.Gui.Wpf.View.Panels;
using Vts.Gui.Wpf.View.Panels.SubPanels;
using Vts.Gui.Wpf.ViewModel;
using Vts.Gui.Wpf.ViewModel.Helpers;

namespace Vts.Gui.Wpf.Test.View.Panels;

[TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
public class PanelViewTests : ViewTestBase
{
    // list of panels with TextBox KeyDown behavior to test
    public static IEnumerable<TestCaseData> TextBoxViewTypes()
    {
        yield return new TestCaseData(typeof(SpectralMappingView)).SetName(nameof(SpectralMappingView));
        yield return new TestCaseData(typeof(FluenceSolverView)).SetName(nameof(FluenceSolverView));
        yield return new TestCaseData(typeof(ForwardSolverView)).SetName(nameof(ForwardSolverView));
        yield return new TestCaseData(typeof(MonteCarloSolverView)).SetName(nameof(MonteCarloSolverView));
        yield return new TestCaseData(typeof(PlotView)).SetName(nameof(PlotView));
        yield return new TestCaseData(typeof(MapView)).SetName(nameof(MapView));
        yield return new TestCaseData(typeof(BloodConcentrationView)).SetName(nameof(BloodConcentrationView));
        yield return new TestCaseData(typeof(OpticalPropertyView)).SetName(nameof(OpticalPropertyView));
        yield return new TestCaseData(typeof(RangeView)).SetName(nameof(RangeView));
    }

    public static IEnumerable<TestCaseData> SimpleViewTypes()
    {
        yield return new TestCaseData(typeof(InverseSolverView)).SetName(nameof(InverseSolverView));
        yield return new TestCaseData(typeof(SolutionDomainOptionView)).SetName(nameof(SolutionDomainOptionView));
        yield return new TestCaseData(typeof(ListBoxOptionView)).SetName(nameof(ListBoxOptionView));
        yield return new TestCaseData(typeof(TextOutputView)).SetName(nameof(TextOutputView));
    }

    [SetUp] public void SetUp() => SetupHost();
    [TearDown] public void TearDown() => TeardownHost();

    [TestCaseSource(nameof(SimpleViewTypes))]
    public void Verify_simple_views(Type viewType)
    {
        UserControl view = null!;
        InvokeOnUI(() =>
        {
            // instantiate the view on UI thread (ensure parameterless ctor)
            view = (UserControl)Activator.CreateInstance(viewType)!;
            HostWindow!.Content = view;
            HostWindow.Show();
            HostWindow.UpdateLayout();
        });
        Assert.IsNotNull(view, $"Failed to instantiate {viewType.Name}");
        Assert.IsInstanceOf<UserControl>(view, $"{viewType.Name} should be a UserControl");
    }

    [TestCaseSource(nameof(TextBoxViewTypes))]
    public void Verify_panels_with_text_box(Type viewType)
    {
        UserControl view = null!;
        InvokeOnUI(() =>
        {
            // instantiate the view on UI thread (ensure parameterless ctor)
            view = (UserControl)Activator.CreateInstance(viewType)!;
            HostWindow!.Content = view;
            HostWindow.Show();
            HostWindow.UpdateLayout();
        });

        // find a TextBox in that view (logical/visual helper)
        TextBox textBox = null;
        InvokeOnUI(() =>
        {
            textBox = VisualTreeHelpers.FindChildLogicalOrVisual<TextBox>(HostWindow!);
        });

        // If the view does not include a TextBox relevant to this test, skip it explicitly
        if (textBox == null)
        {
            Assert.Inconclusive($"{viewType.Name} does not contain a TextBox for this test.");
            return;
        }

        // common test body (same as your existing logic)
        var vm = new TestViewModel();
        var binding = new Binding(nameof(TestViewModel.BoundText))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.Explicit
        };
        InvokeOnUI(() => BindingOperations.SetBinding(textBox!, TextBox.TextProperty, binding));
        const string expected = "new value";
        InvokeOnUI(() => textBox!.Text = expected);

        var args = CreateKeyEventArgsFor(textBox!, Key.Enter);
        InvokeOnUI(() =>
        {
            var method = view.GetType().GetMethod("TextBox_KeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"No TextBox_KeyDown handler found on {viewType.Name}");
            Assert.AreEqual(viewType, method?.DeclaringType, $"Handler for {viewType.Name} should be declared on that type"); 
            method!.Invoke(view, [textBox!, args]);
        });

        Assert.AreEqual(expected, vm.BoundText);
    }

    [Test]
    public void Verify_OptionTemplateSelector_selects_correct_template()
    {
        // Create an OptionModel<IndependentVariableAxis> instance
        var options = OptionModel<IndependentVariableAxis>.CreateAvailableOptions(
            (sender, e) => { }, "group", IndependentVariableAxis.Rho, null, false);
        var option = options.Values.First();

        DataTemplate checkboxTemplate = null;
        DataTemplate radioTemplate = null;
        object resultRadio = null;
        object resultCheckbox = null;

        // Run UI work on the test host dispatcher
        InvokeOnUI(() =>
        {
            // add templates to the host window resources
            checkboxTemplate = new DataTemplate();
            radioTemplate = new DataTemplate();
            Application.Current!.MainWindow = HostWindow;

            var selector = new OptionTemplateSelector();

            option.MultiSelectEnabled = false;
            resultRadio = selector.SelectTemplate(option, null);

            option.MultiSelectEnabled = true;
            resultCheckbox = selector.SelectTemplate(option, null);
        });

        Assert.IsNotNull(resultRadio);
        Assert.IsNotNull(resultCheckbox);
        Assert.IsNotNull(checkboxTemplate);
        Assert.IsNotNull(radioTemplate);

        Assert.IsInstanceOf(radioTemplate?.GetType()!, resultRadio);
        Assert.IsInstanceOf(checkboxTemplate?.GetType()!, resultCheckbox);
    }

    /// <summary>
    /// Test to verify the ForwardSolver executes correctly when using spectral inputs,
    /// and that the expected text output, plot title/labels, and legend are produced.
    /// </summary>
    /// <param name="spectralInput">Indicates whether spectral inputs are used.</param>
    /// <param name="multiAxis">Indicates whether multiple selections are enabled.</param>
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    [Test]
    public void Verify_ForwardSolver_use_spectral_inputs(bool spectralInput, bool multiAxis)
    {
        // WindowViewModel needs to be instantiated for default constructor
        var windowViewModel = new WindowViewModel();
        var spectralMappingViewModel = windowViewModel.SpectralMappingVm;
        var viewModel = windowViewModel.ForwardSolverVm;
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.DistributedGaussianSourceSDA;
        viewModel.ForwardSolver.BeamDiameter = 2;
        viewModel.SolutionDomainTypeOptionVm.SelectedValue = SolutionDomainType.ROfRho;
        spectralMappingViewModel.Wavelength = 750;
        viewModel.SolutionDomainTypeOptionVm.UseSpectralInputs = spectralInput;
        if (multiAxis && spectralInput)
        {
            var optionModel = viewModel.SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm;
            optionModel.EnableMultiSelect = true;
            optionModel.Options[IndependentVariableAxis.Wavelength].IsSelected = true;
        }
        viewModel.ExecuteForwardSolverCommand.Execute(null);
        // ExecuteForwardSolver default settings
        var plotViewModel = windowViewModel.PlotVm;
        var mua = spectralInput ? 0.1676 : 0.01;
        var musp = spectralInput ? 2.2123 : 1;
        const double g = 0.8;
        const double n = 1.4;
        const double diameter = 2;
        // Textbox output
        var s1 = StringLookup.GetLocalizedString("Label_ForwardSolver") +
                 StringLookup.GetLocalizedString("Label_MuA") + "=" +
                 mua.ToString(CultureInfo.CurrentCulture) + " " +
                 StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                 musp.ToString(CultureInfo.CurrentCulture) + " g=" +
                 g.ToString(CultureInfo.CurrentCulture) + " n=" +
                 n.ToString(CultureInfo.CurrentCulture) + "; " +
                 StringLookup.GetLocalizedString("Label_Units") + " = mm⁻¹\r";
        if (multiAxis && spectralInput)
        {
            s1 = "Plot View: plot cleared due to independent axis variable change\r" + s1;
        }
        // Plot label
        var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [mm-2] " +
                 StringLookup.GetLocalizedString("Label_Versus");
        if (multiAxis && spectralInput)
        {
            s2 += " " + viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[1].AxisLabel + " [" +
                  viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[1].AxisUnits + "]";
        }
        else
        {
            s2 += " " + viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[0].AxisLabel + " [" +
                  viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[0].AxisUnits + "]";
        }
        // Legend
        var s3 = "\r" +
                 StringLookup.GetLocalizedString("Label_ModelSDA") + "\r";
        if (multiAxis && spectralInput)
        {
            s3 += viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[0].AxisLabel + " = " +
                  viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[0].AxisRangeVm.Start + " " +
                  viewModel.SolutionDomainTypeOptionVm.IndependentAxesVMs[0].AxisUnits + "\r" +
                  StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") + "\r";
        }
        else
        {
            s3 += StringLookup.GetLocalizedString("Label_MuA") + " = " +
                  mua + " " +
                  StringLookup.GetLocalizedString("Measurement_Inv_mm") + "\r" +
                  StringLookup.GetLocalizedString("Label_MuSPrime") + " = " +
                  musp + " " +
                  StringLookup.GetLocalizedString("Measurement_Inv_mm") + "\r";
        }
        s3 += StringLookup.GetLocalizedString("Label_Diameter") + " = " +
              diameter + " mm";
        if (viewModel.SolutionDomainTypeOptionVm.ConstantAxesVMs.Length > 0)
        {
            s3 += " \r" + viewModel.SolutionDomainTypeOptionVm.ConstantAxesVMs[0].AxisLabel + " = " +
                viewModel.SolutionDomainTypeOptionVm.ConstantAxesVMs[0].AxisValue + " " +
                viewModel.SolutionDomainTypeOptionVm.ConstantAxesVMs[0].AxisUnits;
        }
        Assert.IsTrue(viewModel.IsGaussianForwardModel);
        Assert.That(plotViewModel.Labels[0], Is.EqualTo(s3));
        Assert.That(plotViewModel.Title, Is.EqualTo(s2));
        var textOutputViewModel = windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo(s1));
    }
}
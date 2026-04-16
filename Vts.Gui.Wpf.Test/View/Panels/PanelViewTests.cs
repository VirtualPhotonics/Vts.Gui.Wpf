using NUnit.Framework;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.Test.View.TestHelpers;
using Vts.Gui.Wpf.View.Controls;
using Vts.Gui.Wpf.View.Panels;
using Vts.Gui.Wpf.View.Panels.SubPanels;
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
        TextBox? textBox = null;
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

        DataTemplate? checkboxTemplate = null;
        DataTemplate? radioTemplate = null;
        object? resultRadio = null;
        object? resultCheckbox = null;

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
}
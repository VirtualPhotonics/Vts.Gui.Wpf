using NUnit.Framework;
using System.Windows;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.ViewModel.Helpers;

namespace Vts.Gui.Wpf.Test.ViewModel.Helpers;

[TestFixture]
public class OptionTemplateSelectorTests
{
    [Test, Apartment(ApartmentState.STA), NonParallelizable]
    public void Verify_OptionTemplateSelector_selects_correct_template()
    {
        // Ensure an Application exists for WPF resource lookup
        if (Application.Current == null)
        {
            new Application();
        }

        // Create a Window and add the templates the selector will look up
        var window = new Window();
        var checkboxTemplate = new DataTemplate();
        var radioTemplate = new DataTemplate();
        window.Resources.Add("CheckboxTemplate", checkboxTemplate);
        window.Resources.Add("RadioButtonTemplate", radioTemplate);
        Application.Current?.MainWindow = window;

        // Create an OptionModel<IndependentVariableAxis> instance
        // Provide a no-op handler so CreateAvailableOptions can attach it safely
        var options = OptionModel<IndependentVariableAxis>.CreateAvailableOptions(
            (sender, e) => { }, "group", IndependentVariableAxis.Rho, null, false);
        var option = options.Values.First();

        var selector = new OptionTemplateSelector();

        // When MultiSelectEnabled == false we expect the RadioButtonTemplate
        option.MultiSelectEnabled = false;
        var resultRadio = selector.SelectTemplate(option, null);
        Assert.AreSame(radioTemplate, resultRadio);

        // When MultiSelectEnabled == true we expect the CheckboxTemplate
        option.MultiSelectEnabled = true;
        var resultCheckbox = selector.SelectTemplate(option, null);
        Assert.AreSame(checkboxTemplate, resultCheckbox);
    }
}
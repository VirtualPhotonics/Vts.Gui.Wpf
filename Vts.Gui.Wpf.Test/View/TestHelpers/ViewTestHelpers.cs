using System.Windows;
using System.Windows.Controls;
using Vts.Gui.Wpf.ViewModel.Helpers;

namespace Vts.Gui.Wpf.Test.View.TestHelpers;

public static class ViewTestHelpers
{
    public static void EnsureApplication()
    {
        if (Application.Current == null)
            new Application();
    }

    public static void AddDefaultResources()
    {
        var res = Application.Current!.Resources;

        // simple converter stubs sufficient for XAML parsing in tests
        res["MyResourceToStringConverter"] = new TestConverters.TestResourceToStringConverter();
        res["MyBooleanToVisibilityConverter"] = new TestConverters.TestBooleanToVisibilityConverter();
        res["MyDoubleToStringConverter"] = new TestConverters.TestDoubleToStringConverter();

        // OptionTemplateSelector used by controls in app resources
        res["OptionTemplateSelector"] = new OptionTemplateSelector();

        // simple templates used by selectors/templates in XAML
        res["RadioButtonTemplate"] = CreateSimpleDataTemplate();
        res["CheckboxTemplate"] = CreateSimpleDataTemplate();

        // used by plot and map views in app resources
        res["ScientificFormat"] = "{0:0.##e-0;;0}";
    }

    public static void RemoveDefaultResources()
    {
        var res = Application.Current!.Resources;
        res.Remove("MyResourceToStringConverter");
        res.Remove("MyBooleanToVisibilityConverter");
        res.Remove("MyDoubleToStringConverter");
        res.Remove("OptionTemplateSelector");
        res.Remove("RadioButtonTemplate");
        res.Remove("CheckboxTemplate");
    }

    public static Window CreateHostWindow()
    {
        var window = new Window();
        // ensure window has the same templates in its resources (some code looks up MainWindow.FindResource)
        window.Resources["RadioButtonTemplate"] = CreateSimpleDataTemplate();
        window.Resources["CheckboxTemplate"] = CreateSimpleDataTemplate();
        Application.Current!.MainWindow = window;
        return window;
    }

    private static DataTemplate CreateSimpleDataTemplate()
    {
        var dt = new DataTemplate();
        dt.VisualTree = new FrameworkElementFactory(typeof(TextBlock));
        return dt;
    }
}
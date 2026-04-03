using System.Windows;
using System.Windows.Controls;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Helpers;

public class OptionTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var window = Application.Current.MainWindow;
        //this works for independent axis, but it needs to be more general
        if (item is OptionModel<IndependentVariableAxis> { MultiSelectEnabled: true })
        {
            return window?.FindResource("CheckboxTemplate") as DataTemplate;
        }

        return window?.FindResource("RadioButtonTemplate") as DataTemplate;
    }
}
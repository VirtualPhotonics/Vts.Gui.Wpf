using System.Windows;
using System.Windows.Controls;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.Helpers
{
    public class OptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MultiSelectTemplate { get; set; }

        public DataTemplate SingleSelectTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var window = Application.Current.MainWindow;
            var option = item as OptionModel<IndependentVariableAxis>;
                //this works for independent axis but it needs to be more general
            if ((option != null) && (option.MultiSelectEnabled))
            {
                return window.FindResource("CheckboxTemplate") as DataTemplate;
            }

            return window.FindResource("RadioButtonTemplate") as DataTemplate;
        }
    }
}
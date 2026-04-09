using System.Windows;
using System.Windows.Controls;
using Vts.SpectralMapping;

namespace Vts.Gui.Wpf.View.Templates;

public class ScatteringTemplateSelector : UserControl
{
    public static readonly DependencyProperty ScatteringTypeProperty = DependencyProperty.Register(
        nameof(ScatteringType),
        typeof(string),
        typeof(ScatteringTemplateSelector),
        new PropertyMetadata(string.Empty, UpdateScatteringType));

    public ScatteringTemplateSelector()
    {
        Loaded += (_, _) => ScatteringType = "Vts.SpectralMapping.PowerLawScatterer";
    }

    public DataTemplate MieScatteringTemplate { get; set; }
    public DataTemplate PowerLawScatteringTemplate { get; set; }
    public DataTemplate IntralipidScatteringTemplate { get; set; }

    public string ScatteringType
    { get => (string)GetValue(ScatteringTypeProperty); set => SetValue(ScatteringTypeProperty, value);
    }

    private static void UpdateScatteringType(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var selector = obj as ScatteringTemplateSelector;

        var scattererType = e.NewValue as string;
        if (scattererType == typeof(MieScatterer).FullName)
        {
            selector?.Content = selector.MieScatteringTemplate.LoadContent() as UIElement;
        }
        else if (scattererType == typeof(PowerLawScatterer).FullName)
        {
            selector?.Content = selector.PowerLawScatteringTemplate.LoadContent() as UIElement;
        }
        else if (scattererType == typeof(IntralipidScatterer).FullName)
        {
            selector?.Content = selector.IntralipidScatteringTemplate.LoadContent() as UIElement;
        }
    }
}
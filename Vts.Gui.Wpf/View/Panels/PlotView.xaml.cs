using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vts.Gui.Wpf.ViewModel.Helpers;

namespace Vts.Gui.Wpf.View;

public partial class PlotView : UserControl
{
    public PlotView()
    {
        InitializeComponent();
    }

    private void ExportImage_Click(object sender, RoutedEventArgs e)
    {
        ImageTools.SaveUiElementToPngImage(MyChart);
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tbx && e.Key == Key.Enter)
            tbx.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
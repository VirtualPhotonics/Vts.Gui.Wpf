using System.Windows.Controls;
using System.Windows.Input;

namespace Vts.Gui.Wpf.View.Panels;

public partial class SpectralMappingView : UserControl
{
    public SpectralMappingView()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tbx && e.Key == Key.Enter)
            tbx.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
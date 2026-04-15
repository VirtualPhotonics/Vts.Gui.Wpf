using System.Windows.Controls;
using System.Windows.Input;

namespace Vts.Gui.Wpf.View.Panels.SubPanels;

public partial class BloodConcentrationView : UserControl
{
    public BloodConcentrationView()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tbx && e.Key == Key.Enter)
            tbx.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
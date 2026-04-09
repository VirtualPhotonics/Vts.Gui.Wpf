using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Vts.Gui.Wpf.View;

public partial class FluenceSolutionDomainOptionView : UserControl
{
    public FluenceSolutionDomainOptionView()
    {
        InitializeComponent();
    }

    private void StackPanel_LayoutUpdated(object sender, EventArgs e)
    {
        if (sender is StackPanel stackPanel)
            stackPanel.VerticalAlignment = VerticalAlignment.Top;
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tbx && e.Key == Key.Enter)
            tbx.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
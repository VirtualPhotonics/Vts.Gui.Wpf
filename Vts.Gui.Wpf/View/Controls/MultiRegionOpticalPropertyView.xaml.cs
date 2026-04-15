using System.Windows.Controls;

namespace Vts.Gui.Wpf.View.Controls;

public partial class MultiRegionOpticalPropertyView : UserControl
{

    public MultiRegionOpticalPropertyView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OpListBox.SelectedIndex <= NudRegionIndex.Maximum && OpListBox.SelectedIndex >= NudRegionIndex.Minimum)
        {
            OpListBox.ScrollIntoView(OpListBox.Items[OpListBox.SelectedIndex]);
        }
    }

}
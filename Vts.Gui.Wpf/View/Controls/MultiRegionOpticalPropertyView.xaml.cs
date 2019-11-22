using System.Windows.Controls;

namespace Vts.Gui.Wpf.View
{
    public partial class MultiRegionOpticalPropertyView : UserControl
    {

        public MultiRegionOpticalPropertyView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((opListBox.SelectedIndex <= nudRegionIndex.Maximum) && (opListBox.SelectedIndex >= nudRegionIndex.Minimum))
            {
                opListBox.ScrollIntoView(opListBox.Items[opListBox.SelectedIndex]);
            }
        }

    }
}
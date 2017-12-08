﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Vts.Gui.Wpf.ViewModel;
using Xceed.Wpf.Toolkit;

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
             opListBox.ScrollIntoView(opListBox.Items[opListBox.SelectedIndex]);
        }

    }
}
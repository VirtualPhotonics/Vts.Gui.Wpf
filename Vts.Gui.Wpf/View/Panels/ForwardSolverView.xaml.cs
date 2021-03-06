﻿using System.Windows.Controls;
using System.Windows.Input;

namespace Vts.Gui.Wpf.View
{
    public partial class ForwardSolverView : UserControl
    {
        public ForwardSolverView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tbx = sender as TextBox;
            if (tbx != null && e.Key == Key.Enter)
                tbx.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
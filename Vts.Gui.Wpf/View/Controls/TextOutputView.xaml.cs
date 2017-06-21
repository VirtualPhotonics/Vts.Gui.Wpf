using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.View
{
    public partial class TextOutputView : UserControl
    {
        private TextOutputViewModel _textOutputVM;

        public TextOutputView()
        {
            InitializeComponent();

            Loaded += TextOutputView_Loaded;
        }

        private void TextOutputView_Loaded(object sender, RoutedEventArgs e)
        {
            _textOutputVM = DataContext as TextOutputViewModel;
            if (_textOutputVM != null)
            {
                _textOutputVM.PropertyChanged += textOutputVM_PropertyChanged;
            }
        }

        private void textOutputVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                scrollViewer.ScrollToBottom();
            }
        }

        //    {
        //    if (this.DataContext != null)
        //{


        //public void AppendText(string s)
        //        TextOutputViewModel vm = (TextOutputViewModel)this.DataContext;
        //        if (vm != null)
        //        {
        //            vm.Text += s;
        //        }
        //    }
        //}
    }
}
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
#if !DEBUG
            if (e.PropertyName == "Text")
            {
                // this line throws an error in debug mode so added in a check for debugging
                scrollViewer.ScrollToBottom();
            }
#endif
        }
    }
}
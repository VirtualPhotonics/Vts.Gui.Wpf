using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Vts;
using Vts.Gui.Wpf.Input;

namespace Vts.Gui.Wpf.ViewModel
{
    public class TextOutputViewModel : BindableObject
    {
        private string _Text;

        public TextOutputViewModel()
        {
            TextOutput_PostMessage = new RelayCommand<object>(PostMessage_Executed);
            //Commands.TextOutput_PostMessage.Executed += PostMessage_Executed;
        }

        public RelayCommand<object> TextOutput_PostMessage { get; set; }

        public string Text
        {
            get { return _Text; }
            set 
            {
                _Text = value; 
                OnPropertyChanged("Text");
            }
        }

        void PostMessage_Executed(object sender)
        {
            string s = sender as string;
            if (s != null)
                Text += s;
        }

        public void AppendText(string s)
        {
            Text += s;
        }
    }
}

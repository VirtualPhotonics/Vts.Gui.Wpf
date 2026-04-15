using CommunityToolkit.Mvvm.Input;

namespace Vts.Gui.Wpf.ViewModel.Controls;

public class TextOutputViewModel : BindableObject
{
    public TextOutputViewModel()
    {
        TextOutputPostMessage = new RelayCommand<object>(PostMessage_Executed);
    }

    public RelayCommand<object> TextOutputPostMessage { get; set; }

    public string Text
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    private void PostMessage_Executed(object sender)
    {
        if (sender is string s)
            Text += s;
    }

    public void AppendText(string s)
    {
        Text += s;
    }
}
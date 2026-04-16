namespace Vts.Gui.Wpf.Test.View.TestHelpers;

internal class TestViewModel : System.ComponentModel.INotifyPropertyChanged
{
    public string BoundText
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(BoundText)));
        }
    } = string.Empty;

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
}
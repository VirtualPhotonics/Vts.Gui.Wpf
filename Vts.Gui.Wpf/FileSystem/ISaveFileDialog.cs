namespace Vts.Gui.Wpf.FileSystem
{
    public interface ISaveFileDialog
    {
        string Filter { get; set; }
        bool? ShowDialog();
        string FileName { get; set; }
        string DefaultExtension { get; set; }
    }
}

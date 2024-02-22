namespace Vts.Gui.Wpf.FileSystem
{
    public class SaveFileDialog : ISaveFileDialog
    {
        public string Filter { get; set; }
        public bool? ShowDialog()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            { 
                DefaultExt = DefaultExtension ?? ".txt",
                Filter = Filter
            };
            var result = dialog.ShowDialog();
            FileName = dialog.FileName;
            return result;
        }

        public string FileName { get; set; }
        public string DefaultExtension { get; set; }
    }
}

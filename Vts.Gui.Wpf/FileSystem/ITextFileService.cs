using System;
using System.IO;

namespace Vts.Gui.Wpf.FileSystem
{
    public interface ITextFileService
    {
        public Tuple<FileStream, string> OpenTextFile();
    }
}

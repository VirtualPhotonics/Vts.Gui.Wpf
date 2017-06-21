using Vts.Gui.Wpf.Resources;

namespace Vts.Gui.Wpf.Extensions
{
    /// <summary>
    ///     Class for looking up strings from XAML
    /// </summary>
    public class LocalizedStrings
    {
        private static readonly Strings _resource = new Strings();

        /// <summary>
        ///     MainResource pulls the relevant string from resources
        /// </summary>
        public Strings MainResource
        {
            get { return _resource; }
        }
    }
}
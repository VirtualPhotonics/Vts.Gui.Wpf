using System;
using System.Resources;
using System.Threading;
using Vts.Gui.Wpf.Resources;

namespace Vts.Gui.Wpf.Extensions
{
    /// <summary>
    ///     Class to retrieve strings from a resources file
    /// </summary>
    public static class StringLookup
    {
        /// <summary>
        ///     Method to retrieve the correct language string for the VTS GUI
        /// </summary>
        /// <param name="stringName">The complete name of the string to lookup</param>
        /// <returns>string in the correct language</returns>
        public static string GetLocalizedString(string stringName)
        {
            var rm = new ResourceManager("Vts.Gui.Wpf.Resources.Strings", typeof(Strings).Assembly);

            var s = rm.GetString(stringName, Thread.CurrentThread.CurrentCulture);
            if (s != null)
            {
                return s;
            }
            return "";
        }

        /// <summary>
        ///     Method to retrieve the correct language string for the VTS GUI
        /// </summary>
        /// <param name="stringType">Type of string in the interface(Tooltip, label, title etc)</param>
        /// <param name="stringName">Name of the string</param>
        /// <returns>string in the correct language</returns>
        public static string GetLocalizedString(string stringType, string stringName)
        {
            var baseString = stringType;
            var name = stringName;

            return GetLocalizedString(baseString + "_" + name);
        }

        /// <summary>
        ///     Uses the Enums from the VTS to look up strings for the GUI
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string GetLocalizedString(this Enum enumType)
        {
            var baseString = enumType.GetType().ToString();
            var type = baseString.Substring(baseString.IndexOf('.') + 1);
            var name = enumType.ToString();

            var rm = new ResourceManager("Vts.Gui.Wpf.Resources.Strings", typeof(Strings).Assembly);

            var s = rm.GetString(type + "_" + name, Thread.CurrentThread.CurrentCulture);
            if (s != null)
                return s;
            return "";
        }
    }
}
using Vts.Gui.Wpf.Resources;

namespace Vts.Gui.Wpf.Extensions;

/// <summary>
/// Class for looking up strings from XAML
/// This class will only pull the default resources
/// and will not get the Localized string (despite the name)
/// In XAML:
/// {Binding Path=MainResource.LookupName, Source={StaticResource LocalizedStrings}}
/// </summary>
public class LocalizedStrings
{
    /// <summary>
    ///     MainResource pulls the relevant string from resources
    /// </summary>
    public static Strings MainResource { get; } = new();
}
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Vts.Gui.Wpf.Test.View.TestHelpers;

public static class VisualTreeHelpers
{
    // Try logical tree first, fallback to visual traversal
    public static T? FindChildLogicalOrVisual<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;

        // logical children
        foreach (var childObj in LogicalTreeHelper.GetChildren(parent))
        {
            switch (childObj)
            {
                case T t:
                    return t;
                case DependencyObject dep:
                {
                    var found = FindChildLogicalOrVisual<T>(dep);
                    if (found != null) return found;
                    break;
                }
            }
        }

        // visual fallback - only call VisualTreeHelper on Visual or Visual3D instances
        if (parent is not Visual && parent is not Visual3D) return null;
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T matched) return matched;
                var found = FindChildLogicalOrVisual<T>(child);
                if (found != null) return found;
            }
        }

        return null;
    }
}
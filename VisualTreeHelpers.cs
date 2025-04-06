using System.Windows;
using System.Windows.Media;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Extension methods to help find child elements in visual tree
    /// </summary>
    public static class VisualTreeHelpers
    {
        /// <summary>
        /// Finds a child of the specified type in the visual tree
        /// </summary>
        /// <typeparam name="T">The type of the child to find</typeparam>
        /// <param name="parent">The parent element</param>
        /// <returns>The first child of the specified type or null if no such child exists</returns>
        public static T? FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Extension methods for AutomationElement
    /// </summary>
    public static class AutomationElementExtensions
    {
        /// <summary>
        /// Gets the automation element at the specified point within the given element's scope
        /// </summary>
        public static AutomationElement? FromPoint(this AutomationElement element, Point point)
        {
            try
            {
                // Try to find the element at the specified point within this element's scope
                System.Windows.Automation.Condition condition = new PropertyCondition(
                    AutomationElement.IsControlElementProperty, true);

                AutomationElement? elementAtPoint = null;

                // First check all descendants
                AutomationElementCollection elements = element.FindAll(TreeScope.Descendants, condition);

                foreach (AutomationElement child in elements)
                {
                    try
                    {
                        Rect rect = child.Current.BoundingRectangle;

                        if (rect.Contains(point))
                        {
                            // If we don't have an element yet, or this element is smaller (more specific)
                            if (elementAtPoint == null ||
                                (rect.Width * rect.Height) <
                                (elementAtPoint.Current.BoundingRectangle.Width *
                                 elementAtPoint.Current.BoundingRectangle.Height))
                            {
                                elementAtPoint = child;
                            }
                        }
                    }
                    catch
                    {
                        // Skip elements that cause problems
                    }
                }

                return elementAtPoint;
            }
            catch
            {
                return null;
            }
        }
    }
}

using System;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Represents a UI element in an application that can be automated
    /// </summary>
    public class UIElement
    {
        public string Name { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsTarget { get; set; }
    }
}

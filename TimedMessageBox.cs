
// Progress bar that visually shows the remaining time
// Text indication of how many seconds are left until the window closes
// Dynamic color change of the progress bar (green → orange → red) as the time decreases
// Continuous update of the countdown (every 100ms)
// Proper formatting of numbers in the countdown text
// Smooth color transition from green to orange to red
// Color interpolation is based on remaining time
// Uses RGB value interpolation for smoother color transition

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Class for displaying a message box that automatically closes after a specified time
    /// with visual countdown.
    /// </summary>
    public class TimedMessageBox
    {
        /// <summary>
        /// Configuration options for TimedMessageBox
        /// </summary>
        public class TimedMessageBoxOptions
        {
            public double Width { get; set; } = 400;
            public double Height { get; set; } = 200;
            public int FontSize { get; set; } = 12;
            public bool ShowDropShadow { get; set; } = true;
        }

        /// <summary>
        /// Create a base UI for timed message box
        /// </summary>
        private static Grid CreateMessageBoxContainer(
            string message,
            int timeoutMs,
            TimedMessageBoxOptions? options = null)
        {
            // Default options if not provided
            options ??= new TimedMessageBoxOptions();

            // Create main container
            var container = new Grid();
            container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Text block
            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };
            Grid.SetRow(textBlock, 0);
            container.Children.Add(textBlock);

            // Countdown panel
            var countdownPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20, 0, 20, 20)
            };
            Grid.SetRow(countdownPanel, 1);

            // Countdown text
            var countdownText = new TextBlock
            {
                Text = $"Window will close in {timeoutMs / 1000} seconds",
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
                FontSize = options.FontSize,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };

            // Optional drop shadow
            if (options.ShowDropShadow)
            {
                countdownText.Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 320,
                    ShadowDepth = 1,
                    Opacity = 0.2
                };
            }

            countdownPanel.Children.Add(countdownText);

            // Progress bar
            var progressBar = CreateProgressBar(timeoutMs);
            countdownPanel.Children.Add(progressBar);

            container.Children.Add(countdownPanel);

            return container;
        }

        /// <summary>
        /// Create a styled progress bar
        /// </summary>
        private static ProgressBar CreateProgressBar(int timeoutMs)
        {
            return new ProgressBar
            {
                Minimum = 0,
                Maximum = timeoutMs,
                Value = timeoutMs,
                Height = 2,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(50, 100, 100, 100)),
                Background = new SolidColorBrush(Color.FromArgb(20, 200, 200, 200)),
                Foreground = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Green, 0.0),
                        new GradientStop(Color.FromRgb(0, 200, 0), 1.0)
                    }
                }
            };
        }

        /// <summary>
        /// Compute color based on progress percentage
        /// </summary>
        private static Color ComputeProgressColor(double progressPercentage)
        {
            byte red, green, blue;

            if (progressPercentage > 0.5)
            {
                // From green to orange (0.5 to 1.0)
                double t = (progressPercentage - 0.5) * 2;
                red = (byte)(0 + t * 255);
                green = (byte)(255 - t * 127);
                blue = 0;
            }
            else
            {
                // From orange to red (0.0 to 0.5)
                double t = progressPercentage * 2;
                red = 255;
                green = (byte)(128 * t);
                blue = 0;
            }

            return Color.FromRgb(red, green, blue);
        }

        /// <summary>
        /// Displays a message box with timer and visual countdown.
        /// </summary>
        public static void Show(
            string message,
            string title,
            int timeoutMs,
            TimedMessageBoxOptions? options = null)
        {
            options ??= new TimedMessageBoxOptions();
            var container = CreateMessageBoxContainer(message, timeoutMs, options);

            var msgWindow = new Window
            {
                Title = title,
                Width = options.Width,
                Height = options.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ShowInTaskbar = false,
                Content = container
            };

            var progressBar = VisualTreeHelpers.FindChild<ProgressBar>(container);
            var countdownText = VisualTreeHelpers.FindChild<TextBlock>(container);

            // confirm that the progress bar and countdown text are found
            if (progressBar == null || countdownText == null)
            {
                throw new InvalidOperationException("Failed to find required UI elements in the message box");
            }

            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            var startTime = DateTime.Now;
            var endTime = startTime.AddMilliseconds(timeoutMs);

            updateTimer.Tick += (s, e) =>
            {
                var remaining = (endTime - DateTime.Now).TotalMilliseconds;
                progressBar.Value = Math.Max(0, remaining);

                var secondsRemaining = Math.Ceiling(remaining / 1000);
                countdownText.Text = $"Window will close in {secondsRemaining} seconds";

                double progressPercentage = remaining / timeoutMs;
                progressBar.Foreground = new SolidColorBrush(ComputeProgressColor(progressPercentage));

                if (remaining <= 0)
                {
                    updateTimer.Stop();
                    msgWindow.Close();
                }
            };

            msgWindow.Loaded += (s, e) => updateTimer.Start();
            msgWindow.ShowDialog();
        }

        /// <summary>
        /// Helper class providing methods to search for elements in the WPF visual tree
        /// </summary>
        public static class VisualTreeHelpers
        {
            /// <summary>
            /// Finds a child element of the specified type in the visual tree
            /// </summary>
            /// <typeparam name="T">Type of the element to find</typeparam>
            /// <param name="parent">Parent element to start the search from</param>
            /// <param name="childName">Optional name of the child element to find</param>
            /// <returns>The first child element of the specified type, or null if not found</returns>
            public static T? FindChild<T>(DependencyObject parent, string? childName = null) where T : DependencyObject
            {
                // Verify input parameters
                if (parent == null)
                    return null;

                T? foundChild = null;

                // Search in the children collection
                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    // If this child is of the requested type
                    if (child is T typedChild)
                    {
                        // If child name is specified and it doesn't match, continue
                        if (childName != null && child is FrameworkElement frameworkElement && frameworkElement.Name != childName)
                        {
                            // Continue to search for a child with the requested name
                            continue;
                        }

                        foundChild = typedChild;
                        break;
                    }

                    // Recursively search in the child's children
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break the loop
                    if (foundChild != null)
                        break;
                }

                return foundChild;
            }

            /// <summary>
            /// Finds all child elements of the specified type in the visual tree
            /// </summary>
            /// <typeparam name="T">Type of the elements to find</typeparam>
            /// <param name="parent">Parent element to start the search from</param>
            /// <returns>Collection of all child elements of the specified type</returns>
            public static System.Collections.Generic.List<T> FindAllChildren<T>(DependencyObject parent) where T : DependencyObject
            {
                var result = new System.Collections.Generic.List<T>();
                FindAllChildren(parent, result);
                return result;
            }

            /// <summary>
            /// Helper method for finding all child elements of the specified type
            /// </summary>
            private static void FindAllChildren<T>(DependencyObject parent, System.Collections.Generic.List<T> results) where T : DependencyObject
            {
                // Verify input parameters
                if (parent == null)
                    return;

                // Search in the children collection
                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    // If this child is of the requested type, add it to the results
                    if (child is T typedChild)
                    {
                        results.Add(typedChild);
                    }

                    // Recursively search in the child's children
                    FindAllChildren<T>(child, results);
                }
            }

            /// <summary>
            /// Finds a parent element of the specified type in the visual tree
            /// </summary>
            /// <typeparam name="T">Type of the parent to find</typeparam>
            /// <param name="child">Child element to start the search from</param>
            /// <returns>The first parent element of the specified type, or null if not found</returns>
            public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
            {
                // Get the parent element
                DependencyObject parentObject = VisualTreeHelper.GetParent(child);

                // If we've reached the end of the tree, return null
                if (parentObject == null)
                    return null;

                // Check if the parent is the requested type
                if (parentObject is T parent)
                    return parent;

                // Recursively search up the visual tree
                return FindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// Displays a message box with timer, visual countdown, and custom content.
        /// </summary>
        public static void ShowCustomContent(
            UIElement content,
            string title,
            int timeoutMs,
            TimedMessageBoxOptions? options = null)
        {
            options ??= new TimedMessageBoxOptions();
            var container = CreateMessageBoxContainer(string.Empty, timeoutMs, options);

            // Replace first row with custom content
            container.Children.RemoveAt(0);
            Grid.SetRow(content, 0);
            container.Children.Insert(0, content);

            var msgWindow = new Window
            {
                Title = title,
                Width = options.Width,
                Height = options.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ShowInTaskbar = false,
                Content = container
            };

            var progressBar = VisualTreeHelpers.FindChild<ProgressBar>(container);
            var countdownText = VisualTreeHelpers.FindChild<TextBlock>(container);

            // control for whether the progress bar and text were found
            if (progressBar == null || countdownText == null)
            {
                throw new InvalidOperationException("Failed to find required UI elements in the message box");
            }

            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            var startTime = DateTime.Now;
            var endTime = startTime.AddMilliseconds(timeoutMs);

            updateTimer.Tick += (s, e) =>
            {
                var remaining = (endTime - DateTime.Now).TotalMilliseconds;
                progressBar.Value = Math.Max(0, remaining);

                var secondsRemaining = Math.Ceiling(remaining / 1000);

                if (secondsRemaining == 1)
                    countdownText.Text = $"Window will close in {secondsRemaining} second";
                else 
                    countdownText.Text = $"Window will close in {secondsRemaining} seconds";

                    double progressPercentage = remaining / timeoutMs;
                progressBar.Foreground = new SolidColorBrush(ComputeProgressColor(progressPercentage));

                if (remaining <= 0)
                {
                    updateTimer.Stop();
                    msgWindow.Close();
                }
            };

            msgWindow.Loaded += (s, e) => updateTimer.Start();
            msgWindow.ShowDialog();
        }
    }
}

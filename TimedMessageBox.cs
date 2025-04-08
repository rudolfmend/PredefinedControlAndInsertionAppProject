
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

            // Kontrola, či sa podarilo nájsť progress bar a text
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

            // Kontrola, či sa podarilo nájsť progress bar a text
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
    }
}

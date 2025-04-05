
// Progress bar that visually shows the remaining time
// Text indication of how many seconds are left until the window closes
// Dynamic color change of the progress bar (green → orange → red) as the time decreases
// Continuous update of the countdown (every 100ms)
// Proper formatting of numbers in the countdown text
// Smooth color transition from green to orange to red
// Color interpolation is based on remaining time
// Uses RGB value interpolation for smoother color transition

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
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
        /// Displays a message box with timer and visual countdown.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="title">Window title</param>
        /// <param name="timeoutMs">Time in milliseconds after which the window will close</param>
        /// <param name="width">Window width (optional, default 400)</param>
        /// <param name="height">Window height (optional, default 200)</param>
        public static void Show(string message, string title, int timeoutMs, double width = 400, double height = 200)
        {
            // Create main container
            var container = new Grid();

            // Define rows in grid
            container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Add text block to first row
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

            // Create countdown panel
            var countdownPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20, 0, 20, 20)
            };
            Grid.SetRow(countdownPanel, 1);

            // Add countdown text
            var countdownText = new TextBlock
            {
                Text = $"Window will close in {timeoutMs / 1000} seconds",
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            countdownPanel.Children.Add(countdownText);

            // Add progress bar
            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = timeoutMs,
                Value = timeoutMs,
                Height = 10,
                Foreground = new SolidColorBrush(Colors.Green)
            };
            countdownPanel.Children.Add(progressBar);

            // Add countdown panel to main container
            container.Children.Add(countdownPanel);

            // Create WPF window
            var msgWindow = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                // Pridaný vlastný štýl pre posun krížika
                WindowStyle = WindowStyle.SingleBorderWindow,
                ShowInTaskbar = false,
                Content = container
            };

            // Create DispatcherTimer for updating UI every 100ms
            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            // Set initial time
            var startTime = DateTime.Now;
            var endTime = startTime.AddMilliseconds(timeoutMs);

            updateTimer.Tick += (s, e) =>
            {
                var remaining = (endTime - DateTime.Now).TotalMilliseconds;

                // Update progress bar
                progressBar.Value = Math.Max(0, remaining);

                // Update countdown text
                var secondsRemaining = Math.Ceiling(remaining / 1000);
                countdownText.Text = $"Window will close in {secondsRemaining} seconds";

                // Change progress bar color based on remaining time with gradient effect
                double progressPercentage = remaining / timeoutMs;

                // Interpolate between colors
                byte red, green, blue;

                if (progressPercentage > 0.5)
                {
                    // From green to orange (0.5 to 1.0)
                    double t = (progressPercentage - 0.5) * 2;
                    red = (byte)(0 + t * 255);   // 0 → 255
                    green = (byte)(255 - t * 127);  // 255 → 128
                    blue = 0;
                }
                else
                {
                    // From orange to red (0.0 to 0.5)
                    double t = progressPercentage * 2;
                    red = 255;
                    green = (byte)(128 * t);  // 0 → 128
                    blue = 0;
                }

                progressBar.Foreground = new SolidColorBrush(Color.FromRgb(red, green, blue));

                // Stop timer and close window when time expires
                if (remaining <= 0)
                {
                    updateTimer.Stop();
                    msgWindow.Close();
                }
            };

            msgWindow.Loaded += (s, e) => updateTimer.Start();

            // Show window
            msgWindow.ShowDialog();
        }

        /// <summary>
        /// Displays a message box with timer, visual countdown, and custom content.
        /// </summary>
        /// <param name="content">Custom UI element as window content</param>
        /// <param name="title">Window title</param>
        /// <param name="timeoutMs">Time in milliseconds after which the window will close</param>
        /// <param name="width">Window width (optional, default 400)</param>
        /// <param name="height">Window height (optional, default 200)</param>
        public static void ShowCustomContent(System.Windows.UIElement content, string title, int timeoutMs, double width = 400, double height = 200)
        {
            // Create main container
            var container = new Grid();

            // Define rows in grid
            container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Add content to first row
            Grid.SetRow(content, 0);
            container.Children.Add(content);

            // Create countdown panel
            var countdownPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20, 0, 20, 20)
            };
            Grid.SetRow(countdownPanel, 1);

            // Add countdown text
            var countdownText = new TextBlock
            {
                Text = $"Window will close in {timeoutMs / 1000} seconds",
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            countdownPanel.Children.Add(countdownText);

            // Add progress bar
            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = timeoutMs,
                Value = timeoutMs,
                Height = 10,
                Foreground = new SolidColorBrush(Colors.Green)
            };
            countdownPanel.Children.Add(progressBar);

            // Add countdown panel to main container
            container.Children.Add(countdownPanel);

            // Create WPF window
            var msgWindow = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                // Pridaný vlastný štýl pre posun krížika
                WindowStyle = WindowStyle.SingleBorderWindow,
                ShowInTaskbar = false,
                Content = container
            };

            // Create DispatcherTimer for updating UI every 100ms
            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            // Set initial time
            var startTime = DateTime.Now;
            var endTime = startTime.AddMilliseconds(timeoutMs);

            updateTimer.Tick += (s, e) =>
            {
                var remaining = (endTime - DateTime.Now).TotalMilliseconds;

                // Update progress bar
                progressBar.Value = Math.Max(0, remaining);

                // Update countdown text
                var secondsRemaining = Math.Ceiling(remaining / 1000);
                countdownText.Text = $"Window will close in {secondsRemaining} seconds";

                // Change progress bar color based on remaining time
                if (remaining < timeoutMs * 0.25)
                {
                    progressBar.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (remaining < timeoutMs * 0.5)
                {
                    progressBar.Foreground = new SolidColorBrush(Colors.Orange);
                }

                // Stop timer and close window when time expires
                if (remaining <= 0)
                {
                    updateTimer.Stop();
                    msgWindow.Close();
                }
            };

            msgWindow.Loaded += (s, e) => updateTimer.Start();

            // Show window
            msgWindow.ShowDialog();
        }
    }
}

// --------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace PredefinedControlAndInsertionAppProject
{
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }

    public class EnhancedTimedMessageBox
    {
        public static void Show(string message, string title, int timeoutMs,
            MessageType messageType = MessageType.Information,
            double width = 400,
            double height = 200)
        {
            // Hlavný kontajner s efektami
            var container = new Grid
            {
                Background = GetBackgroundBrush(messageType),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 330,
                    ShadowDepth = 3,
                    Opacity = 0.3
                }
            };

            // Definícia riadkov s pomermi
            container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Ikona podľa typu správy
            var icon = CreateMessageIcon(messageType);
            if (icon != null)
            {
                icon.Width = 50;
                icon.Height = 50;
                icon.Margin = new Thickness(10);
                Grid.SetRow(icon, 0);
                container.Children.Add(icon);
            }

            // Textový blok so štýlovaním
            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(icon != null ? 70 : 20, 20, 20, 20),
                FontSize = 14,
                Foreground = Brushes.Black,
                Effect = new DropShadowEffect
                {
                    Color = Colors.LightGray,
                    Direction = 320,
                    ShadowDepth = 1,
                    Opacity = 0.2
                }
            };
            Grid.SetRow(textBlock, 0);
            container.Children.Add(textBlock);

            // Countdown panel s animáciou
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
                FontWeight = FontWeights.Bold
            };
            countdownPanel.Children.Add(countdownText);

            // Progress bar s animáciou
            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = timeoutMs,
                Value = timeoutMs,
                Height = 15,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray,
                Background = new SolidColorBrush(Color.FromArgb(50, 200, 200, 200))
            };

            // Pridanie svetelnej animácie do progress baru
            var lightAnimation = new DoubleAnimation
            {
                From = 0,
                To = progressBar.Width,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var lightRectangle = new Rectangle
            {
                Width = 50,
                Height = progressBar.Height,
                Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                RenderTransform = new TranslateTransform()
            };

            lightRectangle.RenderTransform.BeginAnimation(TranslateTransform.XProperty, lightAnimation);

            var progressBarGrid = new Grid();
            progressBarGrid.Children.Add(progressBar);
            progressBarGrid.Children.Add(lightRectangle);

            countdownPanel.Children.Add(progressBarGrid);
            container.Children.Add(countdownPanel);

            // Okno s vlastnými nastaveniami
            var msgWindow = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ShowInTaskbar = false,
                Content = container,
                AllowsTransparency = true,
                WindowOpacity = 0.95
            };

            // Zvyšok kódu zostáva rovnaký ako v predchádzajúcej verzii
            // ... (implementácia timera a logiky zatvárania)

            msgWindow.ShowDialog();
        }

        private static UIElement CreateMessageIcon(MessageType type)
        {
            // Jednoduchá implementácia ikon podľa typu správy
            switch (type)
            {
                case MessageType.Information:
                    return new Ellipse
                    {
                        Fill = Brushes.Blue,
                        Stroke = Brushes.DarkBlue,
                        StrokeThickness = 2
                    };
                case MessageType.Warning:
                    return new Polygon
                    {
                        Points = new PointCollection
                        {
                            new Point(25, 0),
                            new Point(50, 50),
                            new Point(0, 50)
                        },
                        Fill = Brushes.Orange,
                        Stroke = Brushes.DarkOrange,
                        StrokeThickness = 2
                    };
                case MessageType.Error:
                    return new Rectangle
                    {
                        Fill = Brushes.Red,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 2
                    };
                default:
                    return null;
            }
        }

        private static Brush GetBackgroundBrush(MessageType type)
        {
            // Gradient pozadia podľa typu správy
            switch (type)
            {
                case MessageType.Information:
                    return new LinearGradientBrush(
                        Color.FromRgb(200, 220, 255),
                        Color.FromRgb(230, 240, 255),
                        90);
                case MessageType.Warning:
                    return new LinearGradientBrush(
                        Color.FromRgb(255, 230, 180),
                        Color.FromRgb(255, 240, 200),
                        90);
                case MessageType.Error:
                    return new LinearGradientBrush(
                        Color.FromRgb(255, 200, 200),
                        Color.FromRgb(255, 220, 220),
                        90);
                default:
                    return Brushes.White;
            }
        }
    }
}
// --------------------------------------------------------------------------
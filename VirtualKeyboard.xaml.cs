using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Interaction logic for VirtualKeyboard.xaml
    /// </summary>
    public partial class VirtualKeyboard : Window
    {
        private bool _isShiftMode = false;
        private bool _isAltGrMode = false;
        private bool _isShortcutCreationMode = false; // Flag for shortcut creation mode

        // Colors for key highlight
        private readonly SolidColorBrush _defaultButtonBrush = new SolidColorBrush(Color.FromRgb(239, 239, 239)); // #EFEFEF
        private readonly SolidColorBrush _specialButtonBrush = new SolidColorBrush(Color.FromRgb(216, 216, 216)); // #D8D8D8
        private readonly SolidColorBrush _highlightedButtonBrush = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // LightBlue
        private readonly SolidColorBrush _modifierButtonBrush = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // LightGreen

        // Set of currently pressed keys for shortcuts
        private HashSet<string> _pressedKeys = new HashSet<string>();
        private const int MAX_SHORTCUT_KEYS = 4; // Maximum number of keys in shortcut

        // Dictionary for Shift key mappings (English layout)
        private Dictionary<string, string> _shiftKeyMappings = new Dictionary<string, string>
        {
            // Numbers and symbols
            { "Oem3", "~" },
            { "D1", "!" },
            { "D2", "@" },
            { "D3", "#" },
            { "D4", "$" },
            { "D5", "%" },
            { "D6", "^" },
            { "D7", "&" },
            { "D8", "*" },
            { "D9", "(" },
            { "D0", ")" },
            { "OemMinus", "_" },
            { "OemPlus", "+" },
            
            // Letters
            { "Q", "Q" },
            { "W", "W" },
            { "E", "E" },
            { "R", "R" },
            { "T", "T" },
            { "Z", "Z" },
            { "U", "U" },
            { "I", "I" },
            { "O", "O" },
            { "P", "P" },
            { "OemOpenBrackets", "{" },
            { "Oem6", "}" },

            { "A", "A" },
            { "S", "S" },
            { "D", "D" },
            { "F", "F" },
            { "G", "G" },
            { "H", "H" },
            { "J", "J" },
            { "K", "K" },
            { "L", "L" },
            { "OemSemicolon", ":" },
            { "Oem7", "\"" },
            { "Oem5", "|" },

            { "Oem102", "|" },
            { "Y", "Y" },
            { "X", "X" },
            { "C", "C" },
            { "V", "V" },
            { "B", "B" },
            { "N", "N" },
            { "M", "M" },
            { "OemComma", "<" },
            { "OemPeriod", ">" },
            { "OemQuestion", "?" }
        };

        // Dictionary for AltGr key mappings
        private Dictionary<string, string> _altGrKeyMappings = new Dictionary<string, string>
        {
            // Special characters and symbols with AltGr
            { "Oem3", "`" },
            { "D1", "¡" },
            { "D2", "²" },
            { "D3", "³" },
            { "D4", "¤" },
            { "D5", "€" },
            { "D6", "¼" },
            { "D7", "½" },
            { "D8", "¾" },
            { "D9", "'" },
            { "D0", "'" },
            { "OemMinus", "–" },
            { "OemPlus", "×" },
            
            // Letters and special characters
            { "Q", "ä" },
            { "W", "å" },
            { "E", "é" },
            { "R", "®" },
            { "T", "þ" },
            { "Z", "ý" },
            { "U", "ú" },
            { "I", "í" },
            { "O", "ó" },
            { "P", "ö" },
            { "OemOpenBrackets", "«" },
            { "Oem6", "»" },

            { "A", "á" },
            { "S", "ß" },
            { "D", "ð" },
            { "F", "đ" },
            { "G", "ŋ" },
            { "H", "ħ" },
            { "J", "ĸ" },
            { "K", "œ" },
            { "L", "ø" },
            { "OemSemicolon", "¶" },
            { "Oem7", "´" },
            { "Oem5", "¬" },

            { "Oem102", "¦" },
            { "Y", "ü" },
            { "X", "»" },
            { "C", "©" },
            { "V", "®" },
            { "B", "'" },
            { "N", "ñ" },
            { "M", "µ" },
            { "OemComma", "ç" },
            { "OemPeriod", "˙" },
            { "OemQuestion", "¿" }
        };

        // Set of modifier keys
        private HashSet<string> _modifierKeys = new HashSet<string>
        {
            "LeftCtrl", "RightCtrl", "LeftShift", "RightShift",
            "LeftAlt", "RightAlt", "LWin", "RWin"
        };

        // Delegate for key press events
        public delegate void KeyPressedEventHandler(string keyName, string keyValue);

        // Event for key press
        public event KeyPressedEventHandler KeyPressed = delegate { };

        // Event for shortcut
        public delegate void ShortcutPressedEventHandler(string[] keys);
        public event ShortcutPressedEventHandler ShortcutPressed = delegate { };

        public VirtualKeyboard()
        {
            InitializeComponent();
            UpdateKeyLabels();
        }

        /// <summary>
        /// Handle button click event
        /// </summary>
        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string keyName = btn.Tag.ToString();
                string keyValue = btn.Content.ToString();

                // Update info text
                txtKeyInfo.Text = $"Key pressed: ({keyName})  {keyValue}";

                // If we're in shortcut creation mode, add keys to the shortcut
                if (_isShortcutCreationMode)
                {
                    // Add to pressed keys if we haven't reached the max
                    if (_pressedKeys.Count < MAX_SHORTCUT_KEYS && !_pressedKeys.Contains(keyName))
                    {
                        _pressedKeys.Add(keyName);

                        // Highlight the button
                        bool isModifier = _modifierKeys.Contains(keyName);
                        HighlightButton(btn, isModifier);

                        // Update the shortcut display
                        UpdateShortcutDisplay();
                    }
                }

                // Always trigger the key press event
                KeyPressed?.Invoke(keyName, keyValue);
            }
        }

        /// <summary>
        /// Highlight a button to indicate it's part of the shortcut
        /// </summary>
        private void HighlightButton(Button button, bool isModifier)
        {
            // Use different colors for modifiers and regular keys
            button.Background = isModifier ? _modifierButtonBrush : _highlightedButtonBrush;
        }

        /// <summary>
        /// Reset a button's color to its default
        /// </summary>
        private void ResetButtonColor(string keyName)
        {
            string buttonName = "btn" + keyName.Replace("Oem", "Oem");
            Button? btn = FindName(buttonName) as Button;

            if (btn != null)
            {
                // Check if it's a special key (different default background)
                bool isSpecialKey = btn.Style.BasedOn != null &&
                                   btn.Style.BasedOn.TargetType == typeof(Button) &&
                                   btn.Style.BasedOn.ToString().Contains("SpecialKeyStyle");

                btn.Background = isSpecialKey ? _specialButtonBrush : _defaultButtonBrush;
            }
            else
            {
                // For buttons without specific name (like "Tab", "Shift", "Enter", etc.)
                foreach (var item in LogicalTreeHelper.GetChildren(this))
                {
                    if (item is Grid grid)
                    {
                        foreach (var gridChild in LogicalTreeHelper.GetChildren(grid))
                        {
                            if (gridChild is StackPanel stackPanel)
                            {
                                foreach (var panelChild in LogicalTreeHelper.GetChildren(stackPanel))
                                {
                                    if (panelChild is Button panelButton && panelButton.Tag.ToString() == keyName)
                                    {
                                        bool isSpecialButton = panelButton.Style.BasedOn != null &&
                                                             panelButton.Style.BasedOn.TargetType == typeof(Button) &&
                                                             panelButton.Style.BasedOn.ToString().Contains("SpecialKeyStyle");

                                        panelButton.Background = isSpecialButton ? _specialButtonBrush : _defaultButtonBrush;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset all button colors to their defaults
        /// </summary>
        private void ClearAllButtonHighlights()
        {
            foreach (string key in _pressedKeys)
            {
                ResetButtonColor(key);
            }
        }

        /// <summary>
        /// Update the shortcut display with current pressed keys
        /// </summary>
        private void UpdateShortcutDisplay()
        {
            if (_pressedKeys.Count == 0)
            {
                txtShortcutInfo.Text = "Shortcut pressed: none";
                return;
            }

            // Build a readable representation of the shortcut
            StringBuilder sb = new StringBuilder();

            // Add each key in the shortcut
            foreach (string key in _pressedKeys)
            {
                // Add a plus sign between keys
                if (sb.Length > 0)
                {
                    sb.Append(" + ");
                }

                // Format the key name for display
                string displayName = FormatKeyNameForDisplay(key);
                sb.Append(displayName);
            }

            txtShortcutInfo.Text = $"Shortcut pressed: {sb}";
        }

        /// <summary>
        /// Format a key name for display in the shortcut info
        /// </summary>
        private string FormatKeyNameForDisplay(string keyName)
        {
            // Map key names to user-friendly display names
            switch (keyName)
            {
                case "LeftCtrl":
                case "RightCtrl":
                    return "Ctrl";

                case "LeftShift":
                case "RightShift":
                    return "Shift";

                case "LeftAlt":
                    return "Alt";

                case "RightAlt":
                    return "AltGr";

                case "LWin":
                case "RWin":
                    return "Win";

                // For letter keys, just return the uppercase letter
                case "A":
                case "B":
                case "C":
                case "D":
                case "E":
                case "F":
                case "G":
                case "H":
                case "I":
                case "J":
                case "K":
                case "L":
                case "M":
                case "N":
                case "O":
                case "P":
                case "Q":
                case "R":
                case "S":
                case "T":
                case "U":
                case "V":
                case "W":
                case "X":
                case "Y":
                case "Z":
                    return keyName;

                // For digit keys, strip the "D" prefix
                case "D0":
                case "D1":
                case "D2":
                case "D3":
                case "D4":
                case "D5":
                case "D6":
                case "D7":
                case "D8":
                case "D9":
                    return keyName.Substring(1); // Remove 'D' prefix

                // Special cases for other keys
                case "OemMinus": return "-";
                case "OemPlus": return "+";
                case "OemOpenBrackets": return "[";
                case "Oem6": return "]";
                case "OemSemicolon": return ";";
                case "Oem7": return "'";
                case "Oem5": return "\\";
                case "Oem102": return "\\";
                case "OemComma": return ",";
                case "OemPeriod": return ".";
                case "OemQuestion": return "/";
                case "Oem3": return "`";
                case "Space": return "Space";
                case "Return": return "Enter";
                case "Back": return "Backspace";
                case "Capital": return "CapsLock";

                // For function keys and others, return as is
                default:
                    return keyName;
            }
        }

        /// <summary>
        /// Clear the current shortcut
        /// </summary>
        public void ClearShortcut()
        {
            ClearAllButtonHighlights();
            _pressedKeys.Clear();
            UpdateShortcutDisplay();
        }

        /// <summary>
        /// Get the current shortcut as an array of key names
        /// </summary>
        public string[] GetCurrentShortcut()
        {
            return _pressedKeys.ToArray();
        }

        /// <summary>
        /// Handle Shift mode change
        /// </summary>
        private void ChkShiftMode_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _isShiftMode = chkShiftMode.IsChecked ?? false;

            // If Shift is enabled, disable AltGr
            if (_isShiftMode && chkAltGrMode.IsChecked == true)
            {
                chkAltGrMode.IsChecked = false;
                _isAltGrMode = false;
            }

            UpdateKeyLabels();
        }

        /// <summary>
        /// Handle AltGr mode change
        /// </summary>
        private void ChkAltGrMode_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _isAltGrMode = chkAltGrMode.IsChecked ?? false;

            // If AltGr is enabled, disable Shift
            if (_isAltGrMode && chkShiftMode.IsChecked == true)
            {
                chkShiftMode.IsChecked = false;
                _isShiftMode = false;
            }

            UpdateKeyLabels();
        }

        /// <summary>
        /// Update key labels based on current mode
        /// </summary>
        private void UpdateKeyLabels()
        {
            // Helper method to update button content
            void UpdateButtonContent(string tag, string normalContent, Dictionary<string, string> mapping)
            {
                string buttonName = "btn" + tag.Replace("Oem", "Oem");
                Button?  btn = FindName(buttonName) as Button;

                if (btn != null)
                {
                    if (_isShiftMode && _shiftKeyMappings.ContainsKey(tag))
                    {
                        btn.Content = _shiftKeyMappings[tag];
                    }
                    else if (_isAltGrMode && _altGrKeyMappings.ContainsKey(tag))
                    {
                        btn.Content = _altGrKeyMappings[tag];
                    }
                    else
                    {
                        btn.Content = normalContent;
                    }

                    // Preserve highlight if the key is part of the shortcut
                    if (_pressedKeys.Contains(tag))
                    {
                        HighlightButton(btn, _modifierKeys.Contains(tag));
                    }
                }
            }

            // Update all keys
            // Numbers and symbols
            UpdateButtonContent("Oem3", "`", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D1", "1", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D2", "2", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D3", "3", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D4", "4", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D5", "5", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D6", "6", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D7", "7", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D8", "8", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D9", "9", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D0", "0", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemMinus", "-", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemPlus", "=", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);

            // Letters - first row
            UpdateButtonContent("Q", _isShiftMode ? "Q" : "q", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("W", _isShiftMode ? "W" : "w", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("E", _isShiftMode ? "E" : "e", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("R", _isShiftMode ? "R" : "r", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("T", _isShiftMode ? "T" : "t", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("Z", _isShiftMode ? "Z" : "z", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("U", _isShiftMode ? "U" : "u", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("I", _isShiftMode ? "I" : "i", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("O", _isShiftMode ? "O" : "o", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("P", _isShiftMode ? "P" : "p", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemOpenBrackets", _isShiftMode ? "{" : "[", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("Oem6", _isShiftMode ? "}" : "]", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);

            // Letters - second row
            UpdateButtonContent("A", _isShiftMode ? "A" : "a", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("S", _isShiftMode ? "S" : "s", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("D", _isShiftMode ? "D" : "d", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("F", _isShiftMode ? "F" : "f", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("G", _isShiftMode ? "G" : "g", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("H", _isShiftMode ? "H" : "h", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("J", _isShiftMode ? "J" : "j", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("K", _isShiftMode ? "K" : "k", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("L", _isShiftMode ? "L" : "l", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemSemicolon", _isShiftMode ? ":" : ";", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("Oem7", _isShiftMode ? "\"" : "'", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("Oem5", _isShiftMode ? "|" : "\\", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);

            // Letters - third row
            UpdateButtonContent("Oem102", _isShiftMode ? "|" : "\\", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("Y", _isShiftMode ? "Y" : "y", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("X", _isShiftMode ? "X" : "x", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("C", _isShiftMode ? "C" : "c", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("V", _isShiftMode ? "V" : "v", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("B", _isShiftMode ? "B" : "b", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("N", _isShiftMode ? "N" : "n", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("M", _isShiftMode ? "M" : "m", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemComma", _isShiftMode ? "<" : ",", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemPeriod", _isShiftMode ? ">" : ".", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
            UpdateButtonContent("OemQuestion", _isShiftMode ? "?" : "/", _isShiftMode ? _shiftKeyMappings : _altGrKeyMappings);
        }

        /// <summary>
        /// Get the last pressed key
        /// </summary>
        /// <returns>Information about the key in the format (Key name, value)</returns>
        public Tuple<string, string> GetLastPressedKey()
        {
            string keyInfo = txtKeyInfo.Text;

            if (keyInfo == "Key pressed: none")
            {
                return new Tuple<string, string>(string.Empty, string.Empty);
            }

            // Parse information from TextBlock
            try
            {
                // Format: "Key pressed: (KEYNAME) KEYVALUE"
                int startIndex = keyInfo.IndexOf("(") + 1;
                int endIndex = keyInfo.IndexOf(")");

                if (startIndex > 0 && endIndex > startIndex)
                {
                    string keyName = keyInfo.Substring(startIndex, endIndex - startIndex);
                    string keyValue = keyInfo.Substring(endIndex + 2).Trim(); // Skip ") " and trim
                    return new Tuple<string, string>(keyName, keyValue);
                }
            }
            catch (Exception)
            {
                // Return empty values in case of error
                Console.WriteLine("Error parsing key info");
            }

            return new Tuple<string, string>(string.Empty, string.Empty);
        }

        /// <summary>
        /// Toggle shortcut creation mode
        /// </summary>
        private void ButtonCreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Toggle shortcut creation mode
                _isShortcutCreationMode = !_isShortcutCreationMode;

                if (_isShortcutCreationMode)
                {
                    // Entering shortcut creation mode
                    ButtonCreateShortcut.Content = "Finish Creating Shortcut";
                    ButtonCreateShortcut.Background = Brushes.LightGreen;

                    // Clear any existing shortcut
                    ClearShortcut();

                    // Update instruction
                    txtShortcutInfo.Text = "Press keys to create a shortcut...";
                }
                else
                {
                    // Exiting shortcut creation mode
                    ButtonCreateShortcut.Content = "Start Creating Shortcut";
                    ButtonCreateShortcut.Background = _defaultButtonBrush;

                    // If we have keys in the shortcut, trigger the event
                    string[] shortcutKeys = GetCurrentShortcut();
                    if (shortcutKeys.Length > 0)
                    {
                        // Trigger the shortcut pressed event
                        ShortcutPressed?.Invoke(shortcutKeys);

                        // Visual feedback - blink the shortcut info
                        BlinkShortcutInfo();
                    }
                    else
                    {
                        // No keys were added
                        txtShortcutInfo.Text = "Shortcut pressed: none";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling shortcut mode: {ex.Message}");
            }
        }

        /// <summary>
        /// Visual feedback effect when shortcut is created
        /// </summary>
        private async void BlinkShortcutInfo()
        {
            // Store original foreground
            Brush originalBrush = txtShortcutInfo.Foreground;

            // Blink effect
            txtShortcutInfo.Foreground = Brushes.Green;

            // Wait 500ms
            await System.Threading.Tasks.Task.Delay(500);

            // Restore original foreground
            txtShortcutInfo.Foreground = originalBrush;
        }
    }
}

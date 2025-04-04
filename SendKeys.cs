using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Provides methods for sending keystrokes to the active application
    /// </summary>
    public static class SendKeys
    {
        // Required Win32 API functions
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // Virtual key codes
        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_ALT = 0x12;
        private const byte VK_BACK = 0x08;
        private const byte VK_TAB = 0x09;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_ESCAPE = 0x1B;
        private const byte VK_DELETE = 0x2E;
        private const byte VK_HOME = 0x24;
        private const byte VK_END = 0x23;
        private const byte VK_LEFT = 0x25;
        private const byte VK_UP = 0x26;
        private const byte VK_RIGHT = 0x27;
        private const byte VK_DOWN = 0x28;
        private const byte VK_F1 = 0x70;

        // Keyboard event flags
        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// Sends the specified text to the active window
        /// </summary>
        /// <param name="text">Text with special key indicators like {ENTER}, ^a (Ctrl+A), etc.</param>
        public static void SendWait(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Process the text, looking for special key sequences
            for (int i = 0; i < text.Length; i++)
            {
                // Check for special key sequences
                if (text[i] == '{' && i < text.Length - 1)
                {
                    // Find closing brace
                    int closeBrace = text.IndexOf('}', i + 1);
                    if (closeBrace > i)
                    {
                        string keyName = text.Substring(i + 1, closeBrace - i - 1).ToUpper();
                        SendSpecialKey(keyName);
                        i = closeBrace;
                        continue;
                    }
                }
                else if (text[i] == '^' && i < text.Length - 1)
                {
                    // Control key sequence (e.g., ^a for Ctrl+A)
                    SendControlKeyCombo(text[i + 1]);
                    i++;
                    continue;
                }
                else if (text[i] == '%' && i < text.Length - 1)
                {
                    // Alt key sequence (e.g., %f for Alt+F)
                    SendAltKeyCombo(text[i + 1]);
                    i++;
                    continue;
                }
                else if (text[i] == '+' && i < text.Length - 1)
                {
                    // Shift key sequence (e.g., +a for Shift+A)
                    SendShiftKeyCombo(text[i + 1]);
                    i++;
                    continue;
                }

                // Regular character, just send it
                SendChar(text[i]);
            }

            // Small delay after sending keys to ensure they are processed
            Thread.Sleep(50);
        }

        private static void SendChar(char c)
        {
            // Convert char to virtual key code
            short vkey = VkKeyScan(c);
            byte keyCode = (byte)(vkey & 0xFF);
            bool shift = (vkey & 0x100) != 0;

            // Press Shift if needed
            if (shift)
                keybd_event(VK_SHIFT, 0, 0, 0);

            // Press and release the key
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0);

            // Release Shift if it was pressed
            if (shift)
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);

            // Small delay to simulate realistic typing speed
            Thread.Sleep(10);
        }

        private static void SendSpecialKey(string keyName)
        {
            byte keyCode = 0;
            bool isExtended = false;

            switch (keyName)
            {
                case "ENTER":
                case "RETURN":
                    keyCode = VK_RETURN;
                    break;
                case "TAB":
                    keyCode = VK_TAB;
                    break;
                case "ESC":
                case "ESCAPE":
                    keyCode = VK_ESCAPE;
                    break;
                case "BACKSPACE":
                case "BS":
                    keyCode = VK_BACK;
                    break;
                case "DELETE":
                case "DEL":
                    keyCode = VK_DELETE;
                    isExtended = true;
                    break;
                case "HOME":
                    keyCode = VK_HOME;
                    isExtended = true;
                    break;
                case "END":
                    keyCode = VK_END;
                    isExtended = true;
                    break;
                case "LEFT":
                    keyCode = VK_LEFT;
                    isExtended = true;
                    break;
                case "RIGHT":
                    keyCode = VK_RIGHT;
                    isExtended = true;
                    break;
                case "UP":
                    keyCode = VK_UP;
                    isExtended = true;
                    break;
                case "DOWN":
                    keyCode = VK_DOWN;
                    isExtended = true;
                    break;
                default:
                    // Check if it's a function key (F1-F12)
                    if (keyName.Length >= 2 && keyName[0] == 'F')
                    {
                        if (int.TryParse(keyName.Substring(1), out int fKeyNum) && fKeyNum >= 1 && fKeyNum <= 12)
                        {
                            keyCode = (byte)(VK_F1 + fKeyNum - 1);
                        }
                    }
                    break;
            }

            if (keyCode != 0)
            {
                // Send the special key
                int flags = isExtended ? KEYEVENTF_EXTENDEDKEY : 0;
                keybd_event(keyCode, 0, flags, 0);
                keybd_event(keyCode, 0, flags | KEYEVENTF_KEYUP, 0);
                Thread.Sleep(50); // Slightly longer delay for special keys
            }
        }

        private static void SendControlKeyCombo(char key)
        {
            // Convert char to virtual key code
            short vkey = VkKeyScan(key);
            byte keyCode = (byte)(vkey & 0xFF);

            // Press Ctrl
            keybd_event(VK_CONTROL, 0, 0, 0);

            // Press and release the key
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0);

            // Release Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

            Thread.Sleep(50);
        }

        private static void SendAltKeyCombo(char key)
        {
            // Convert char to virtual key code
            short vkey = VkKeyScan(key);
            byte keyCode = (byte)(vkey & 0xFF);

            // Press Alt
            keybd_event(VK_ALT, 0, 0, 0);

            // Press and release the key
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0);

            // Release Alt
            keybd_event(VK_ALT, 0, KEYEVENTF_KEYUP, 0);

            Thread.Sleep(50);
        }

        private static void SendShiftKeyCombo(char key)
        {
            // Convert char to virtual key code
            short vkey = VkKeyScan(key);
            byte keyCode = (byte)(vkey & 0xFF);

            // Press Shift
            keybd_event(VK_SHIFT, 0, 0, 0);

            // Press and release the key
            keybd_event(keyCode, 0, 0, 0);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0);

            // Release Shift
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);

            Thread.Sleep(50);
        }

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);
    }
}

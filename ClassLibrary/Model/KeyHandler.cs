using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CefSharp;

namespace CashierLibrary.Model
{
    public class KeyHandler : IKeyboardHandler
    {
        public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            return false;
        }

        public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            if (windowsKeyCode == System.Windows.Forms.Keys.F10.GetHashCode())
            {
                browser.ShowDevTools();
            }
            return false;
        }
    }
}

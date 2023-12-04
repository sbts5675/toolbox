using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SharedUtil
{
    internal class KeyControl
    {

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

        private const int KEYEVENTF_KEYDOWN = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;


        public static void keyDown(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, IntPtr.Zero);
        }

        public static void keyUp(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

    }
}

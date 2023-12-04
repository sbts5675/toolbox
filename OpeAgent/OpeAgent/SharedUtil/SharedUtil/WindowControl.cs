using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SharedUtil
{
    internal class WindowControl
    {


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool DestroyWindow(IntPtr hWnd);


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public enum WindowRectResultPattern : int
        {
            Rect,
            TopLeft,
            Middle,
            BottomRight
        }



        /**
         * 最前面のアプリのタイトルを取得する
         * 例 Edgeの場合「C# Coding in Visual Studio - 個人 - Microsoft? Edge」
         */
        static public string getForegroundAppName()
        {
            string foregroundAppName = "";
            IntPtr handle = GetForegroundWindow();
            const int capacity = 256;
            StringBuilder stringBuilder = new StringBuilder(capacity);
            if (GetWindowText(handle, stringBuilder, capacity) > 0)
            {
                foregroundAppName = stringBuilder.ToString();
            }
            if (GetClassName(handle, stringBuilder, capacity) > 0)
            {
                foregroundAppName = foregroundAppName + "/" + stringBuilder.ToString();
            }

            return foregroundAppName;
        }

        static public bool setWindowToForeground(string title)
        {
            bool result = false;
            IntPtr handle = FindWindow(null, title);

            // タイトルでハンドルが見つかった場合
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);

                // 最前面のタイトルと一致した場合(一致しない場合はなぜかうまくいかなかった場合)
                if (title.Equals(getForegroundAppName()))
                {
                    result = true;
                }
            }

            return result;
        }

        public static string getWindowRect(string title, WindowRectResultPattern resultType)
        {
            string result = "-1,-1,-1,-1";
            IntPtr handle = FindWindow(null, title);

            // タイトルでハンドルが見つかった場合
            if (handle != IntPtr.Zero)
            {
                RECT rect = new RECT();
                GetWindowRect(handle, out rect);

                switch (resultType)
                {
                    case WindowRectResultPattern.TopLeft:
                        result = string.Format("{0},{1}", rect.Left, rect.Top);
                        break;
                    case WindowRectResultPattern.Middle:
                        result = string.Format("{0},{1}", (int)((rect.Left + rect.Right) / 2), (int)((rect.Top + rect.Bottom) / 2));
                        break;
                    case WindowRectResultPattern.Rect:
                        result = string.Format("{0},{1},{2},{3}", rect.Left, rect.Top, rect.Right, rect.Bottom);
                        break;
                    case WindowRectResultPattern.BottomRight:
                        result = string.Format("{0},{1}", rect.Right, rect.Bottom);
                        break;
                    default:
                        throw new ProcessorException(string.Format("システムエラー getWindowRectに対する引数{0}", resultType));

                }

            }

            return result;
        }

        public static void closeWindow(string title)
        {
            IntPtr handle = FindWindow(null, title);

            // タイトルでハンドルが見つかった場合
            if (handle != IntPtr.Zero)
            {
                if (DestroyWindow(handle))
                {
                    Console.WriteLine("ウィンドウを破棄しました。");
                }
                else
                {
                    Console.WriteLine("ウィンドウの破棄に失敗しました。エラーコード: " + Marshal.GetLastWin32Error());
                }
            }
            else {
                Console.WriteLine("No Window");

            }

        }
    }
}

using OpenCvSharp.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SharedUtil
{
    internal class MouseControl
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);



        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;
        private const int MOUSEEVENTF_WHEEL = 0x0800; // マウスホイールイベント

        public const int BUTTON_LEFT = 0;
        public const int BUTTON_RIGHT = 1;
        public const int BUTTON_MIDDLE = 2;

        public static void click(int button) 
        {
            int downEvent = 0;
            int upEvent = 0;
        

            switch (button) 
            {
                case BUTTON_LEFT:
                    downEvent |= MOUSEEVENTF_LEFTDOWN;
                    upEvent |= MOUSEEVENTF_LEFTUP;
                    break;

                case BUTTON_RIGHT:
                    downEvent |= MOUSEEVENTF_RIGHTDOWN;
                    upEvent |= MOUSEEVENTF_RIGHTUP;
                    break;

                case BUTTON_MIDDLE:
                    downEvent |= MOUSEEVENTF_MIDDLEDOWN;
                    upEvent |= MOUSEEVENTF_MIDDLEUP;
                    break;
            }

            mouse_event((uint)downEvent, 0, 0, 0, 0);
            Thread.Sleep(200);
            mouse_event((uint)upEvent, 0, 0, 0, 0);


        }


        public static void scroll(int scrollAmount)
        {
            // scrollAmountが正の場合、上方向へのスクロール
            // scrollAmountが負の場合、下方向へのスクロール
            // 前処理のfunctionScrollの中で正負を逆転させている

            int abs = Math.Abs(scrollAmount); // ループ用の絶対値

            int direction = scrollAmount / abs; // 方向を求めるため +1 or -1 にする

            int scrollValue = 120 * direction; // 1回のスクロールで120単位進む


            // なぜか5以上のスクロールは反映されないので、１スクロールを繰り返す
            for (int i=0; i<abs; i++)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)scrollValue, 0);
                Thread.Sleep(100);

            }


            /*
                        INPUT input = new INPUT
                        {
                            type = 0, // 0:マウス 1:キーボード 2:ハードウェア
                            ui = new INPUT_UNION
                            {
                                mouse = new MOUSEINPUT
                                {
                                    dwFlags = MOUSEEVENTF_WHEEL,
                                    dx = x,
                                    dy = y,
                                    mouseData = scrollValue,
                                    dwExtraInfo = IntPtr.Zero,
                                    time = 0
                                }
                            }
                        };


                        // マウススクロールイベントを送信
                        SendInput(1, ref input, Marshal.SizeOf(input));
            */
        }



    }
}

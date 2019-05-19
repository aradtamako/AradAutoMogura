using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace AradAutoMogura
{
    class CoordinateInfo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public VirtualKeyCode VirtualKeyCode { get; set; }

        public CoordinateInfo(int x, int y, VirtualKeyCode virtualKeyCode)
        {
            X = x;
            Y = y;
            VirtualKeyCode = virtualKeyCode;
        }
    }

    class Program
    {
        const int Width = 1280;
        const int Height = 960;
        static InputSimulator simulator = new InputSimulator();
        static Bitmap bmp;
        static Graphics g;

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        static void PressKey(VirtualKeyCode keyCode)
        {
            simulator.Keyboard.KeyDown(keyCode);
            Thread.Sleep(100);
            simulator.Keyboard.KeyUp(keyCode);
        }

        static Process GetAradProcess()
        {
            var process = Process.GetProcessesByName("ARAD");

            if (!process.Any())
            {
                throw new InvalidOperationException("ARAD.exe is not running");
            }

            return process.First();
        }

        static void Start()
        {
            while (true)
            {
                if ((GetAsyncKeyState(Keys.Escape) & 0x8000) > 0)
                {
                    Environment.Exit(0);
                }
                
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

                var cordinates = new List<CoordinateInfo>()
                {
                    new CoordinateInfo(460, 530, VirtualKeyCode.VK_Q),
                    new CoordinateInfo(640, 530, VirtualKeyCode.VK_W),
                    new CoordinateInfo(820, 530, VirtualKeyCode.VK_E),

                    new CoordinateInfo(460, 630, VirtualKeyCode.VK_A),
                    new CoordinateInfo(640, 630, VirtualKeyCode.VK_S),
                    new CoordinateInfo(820, 630, VirtualKeyCode.VK_D),

                    new CoordinateInfo(460, 730, VirtualKeyCode.VK_Z),
                    new CoordinateInfo(640, 730, VirtualKeyCode.VK_X),
                    new CoordinateInfo(820, 730, VirtualKeyCode.VK_C),
                };

                foreach (var cordinate in cordinates)
                {
                    var c = bmp.GetPixel(cordinate.X, cordinate.Y);
                    if (c.R == 239 && c.G == 206 && c.B == 173)
                    {
                        PressKey(cordinate.VirtualKeyCode);
                    }
                }

                Thread.Sleep(10);
            }
        }

        static void Main(string[] args)
        {
            bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(bmp);

            var process = GetAradProcess();
            var pid = process.Id;
            var hWnd = process.MainWindowHandle;

            if (hWnd != IntPtr.Zero)
            {
                GetClientRect(hWnd, out var rect);

                if (!(rect.Width == Width && rect.Height == Height))
                {
                    Console.WriteLine("Please set resolution 1280x960");
                    return;
                }

                MoveWindow(hWnd, 0, 0, rect.Width, rect.Height, true);
                Microsoft.VisualBasic.Interaction.AppActivate(pid);
                Thread.Sleep(500);

                Start();
            }
        }
    }
}

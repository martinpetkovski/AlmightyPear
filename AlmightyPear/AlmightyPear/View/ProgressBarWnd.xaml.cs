using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;

namespace Checkmeg.WPF.View
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class ProgressBarWnd : MetroWindow
    {
        public static ProgressBarWnd Instance { get; set; }
        private double actualProgress;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static void UpdateProgress(double progress, string status)
        {
            if (Instance != null)
            {
                Instance.pb_Progress.Value = progress;
                Instance.actualProgress = progress;
            }
        }

        public ProgressBarWnd()
        {
            InitializeComponent();
        }

        private async Task PollProgressAsync()
        {
            while (Instance.actualProgress < 1)
            {
                await Task.Delay(100);
            }
        }

        public static async void FireAsync()
        {
            if(Instance != null)
            {
                Instance.Hide();
                Instance = null;
            }
            Instance = new ProgressBarWnd();
            Point mousePos = GetMousePosition();
            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y));

            Instance.Left = mousePos.X;
            Instance.Top = mousePos.Y;

            Instance.Show();

            await Task.Run(async () =>
            {
                await Instance.PollProgressAsync();
            });

            Instance.Hide();
            Instance = null;
        }
    }
}

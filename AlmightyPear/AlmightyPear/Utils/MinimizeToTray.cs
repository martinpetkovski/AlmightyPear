using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace AlmightyPear.Utils
{

    public static class MinimizeToTray
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static System.Drawing.Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }

        public static void Register(Window window)
        {
            new MinimizeToTrayInstance(window);
        }

        private class MinimizeToTrayInstance
        {
            private Window _window;
            private NotifyIcon _notifyIcon;
            private bool _balloonShown;


            public MinimizeToTrayInstance(Window window)
            {
                Debug.Assert(window != null, "window parameter is null.");
                _window = window;
                _window.StateChanged += new EventHandler(HandleStateChanged);
            }

            private void HandleStateChanged(object sender, EventArgs e)
            {
                if (_notifyIcon == null)
                {
                    _notifyIcon = new NotifyIcon();
                    _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                    _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
                    _notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
                }
                _notifyIcon.Text = _window.Title;

                var minimized = (_window.WindowState == WindowState.Minimized);
                _window.ShowInTaskbar = !minimized;
                _notifyIcon.Visible = minimized;
                if (minimized && !_balloonShown)
                {
                    _notifyIcon.ShowBalloonTip(1000, null, _window.Title, ToolTipIcon.None);
                    _balloonShown = true;
                }
            }

            private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
            {
                MouseEventArgs ea = (MouseEventArgs)e;
                if (ea.Button == MouseButtons.Left)
                {
                    _window.WindowState = WindowState.Normal;
                }
                else if(ea.Button == MouseButtons.Right)
                {
                    System.Windows.Controls.ContextMenu menu = (System.Windows.Controls.ContextMenu)_window.FindResource("NotifierContextMenu");
                    menu.IsOpen = true;
                }
            }
        }
    }
}

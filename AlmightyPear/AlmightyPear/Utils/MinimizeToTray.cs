using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace AlmightyPear.Utils
{

    public static class MinimizeToTray
    {

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
                _window.WindowState = WindowState.Normal;
            }
        }
    }
}

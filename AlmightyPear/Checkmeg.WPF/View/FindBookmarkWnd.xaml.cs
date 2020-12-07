﻿using Checkmeg.WPF.Controller;
using Core;
using MahApps.Metro.Controls;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Checkmeg.WPF.View
{
    public partial class FindBookmarkWnd : MetroWindow
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
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public FindBookmarkWnd()
        {
            InitializeComponent();

            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        public void Fire(string filter = "")
        {
            if (!Engine.Env.UserData.IsLoggedIn)
                return;

            Point mousePos = GetMousePosition();
            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y));

            double finalW = screen.Bounds.Width / 2;

            double finalX = (screen.Bounds.X + (screen.Bounds.Width / 2)) - (finalW / 2);
            double finalY = (screen.Bounds.Y + (screen.Bounds.Height / 2)) - Height / 2;
            
            Width = finalW;
            Left = finalX;
            Top = finalY;

            ctrl_filter.tb_filter.Text = filter;

            if (Engine.Env.UserData.CustomModel.AnimationsLevel == 2)
            {
                mah_contentControl.TransitionsEnabled = true;
            }
            else
            {
                mah_contentControl.TransitionsEnabled = false;
            }
            Show();
            Activate();
            mah_contentControl.Reload();
        }

        private void Ctrl_filter_FilterEvent(object sender, EventArgs e)
        {
            System.Windows.Input.KeyEventArgs ea = (System.Windows.Input.KeyEventArgs)e;

            if (ea.Key == Key.Escape)
            {
                ClipboardManager.ClearClipboard();
                Hide();
            }
        }

        private void MetroWindow_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
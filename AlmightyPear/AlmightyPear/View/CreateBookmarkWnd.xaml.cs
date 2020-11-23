﻿using AlmightyPear.Controller;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

namespace AlmightyPear.View
{
    public partial class CreateBookmarkWnd : Window
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

        public CreateBookmarkWnd()
        {
            InitializeComponent();

            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        public void Fire()
        {
            if (!Env.UserData.IsLoggedIn)
                return;

            InputSimulator inputSim = new InputSimulator();

            inputSim.Keyboard.KeyUp(VirtualKeyCode.LWIN);
            inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

            ctrl_bookmarkCreate.Initialize();

            Point mousePos = GetMousePosition();
            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y));

            double finalW = screen.Bounds.Width / 2;
            double finalH = 200;

            double finalX = Math.Abs((screen.Bounds.X + (screen.Bounds.Width / 2)) - finalW / 2);
            double finalY = Math.Abs((screen.Bounds.Y + (screen.Bounds.Height / 2)) - finalH / 2);

            Width = finalW;
            Height = finalH;
            Left = finalX;
            Top = finalY;

            Show();
            Activate();
        }


        private async void Ctrl_bookmarkCreate_InputEventAsync(object sender, EventArgs e)
        {
            System.Windows.Input.KeyEventArgs ea = (System.Windows.Input.KeyEventArgs)e;
            if (ea.Key == Key.Enter)
            {
                await ctrl_bookmarkCreate.CreateAsync();
                Hide();
            }
            else if (ea.Key == Key.Escape)
            {
                ctrl_bookmarkCreate.Cancel();
                Hide();
            }
        }
    }
}

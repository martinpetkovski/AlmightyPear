﻿using MahApps.Metro.Controls;
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
    public partial class MessageBox : MetroWindow
    {
        private static MessageBox Instance { get; set; }

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

        private int selected = -1;

        private void ButtonClicked(object source, EventArgs e)
        {
            Button btn = (Button)source;
            selected = btn.TabIndex;
        }

        public MessageBox(string title, string message, List<string> buttons)
        {
            InitializeComponent();
            this.Title = title;
            txt_message.Text = message;

            int i = 0;
            foreach (string buttonLabel in buttons)
            {
                Button current = new Button();
                current.Content = buttonLabel;
                current.Click += ButtonClicked;
                current.TabIndex = i;
                current.Padding = new Thickness(10);
                current.Margin = new Thickness(10);

                sp_buttonContainer.Children.Add(current);
                i++;
            }
        }

        private async Task PollSelectionAsync()
        {
            while (selected == -1)
            {
                await Task.Delay(100);
            }
        }

        private static double Clamp(double x, double a, double b)
        {
            return Math.Min(Math.Max(x, a), b);
        }

        public static async Task<int> FireAsync(string title, string message, List<string> buttons)
        {
            if(Instance != null)
            {
                Instance.Hide();
                Instance = null;
            }
            Instance = new MessageBox(title, message, buttons);
            Point mousePos = GetMousePosition();
            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y));

            Instance.Left = Clamp(mousePos.X - 320, screen.Bounds.Left, screen.Bounds.Left + screen.Bounds.Width - Instance.Width);
            Instance.Top  = Clamp(mousePos.Y - 160, screen.Bounds.Top, screen.Bounds.Top + screen.Bounds.Height - Instance.Height);

            Instance.Show();

            await Task.Run(async () =>
            {
                await Instance.PollSelectionAsync();
            });

            Instance.Hide();
            int retVal = Instance.selected;
            Instance = null;
            return retVal;
        }
    }
}
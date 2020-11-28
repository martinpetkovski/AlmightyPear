using AlmightyPear.Controller;
using AlmightyPear.Model;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlmightyPear.View
{
    /// <summary>
    /// Interaction logic for BookmarkDetailsWnd.xaml
    /// </summary>
    public partial class BookmarkDetailsWnd : MetroWindow
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

        public BookmarkModel Bookmark { get; set; }

        public BookmarkDetailsWnd(BookmarkModel bookmark)
        {
            Bookmark = bookmark;
            InitializeComponent();

            Point mousePos = GetMousePosition();
            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y));

            double finalW = screen.Bounds.Width / 2;
            double finalH = screen.Bounds.Height / 4;

            double finalX = (screen.Bounds.X + (screen.Bounds.Width / 2)) - (finalW / 2);
            double finalY = (screen.Bounds.Y + (screen.Bounds.Height / 2)) - (finalH / 2);

            Width = finalW;
            Height = finalH;
            Left = finalX;
            Top = finalY;

            tb_content.Text = Bookmark.Content;
            Env.ExplicitFocus(tb_path);
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Accept()
        {
            Env.BinController.ChangePath(Bookmark, tb_path.Text);
            Bookmark.Content = tb_content.Text;
            Close();
        }

        private void Btn_accept_Click(object sender, RoutedEventArgs e)
        {
            Accept();
        }

        private void Tb_content_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Accept();
            }
        }

        private void Tb_path_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Accept();
            }
        }
    }
}

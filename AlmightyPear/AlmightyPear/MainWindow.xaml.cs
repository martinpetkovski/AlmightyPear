using AlmightyPear.Controller;
using AlmightyPear.Model;
using AlmightyPear.Utils;
using AlmightyPear.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace AlmightyPear
{
    public partial class MainWindow : MetroWindow
    {
        public enum EHotWnd : int
        {
            CreateBookmark = 0,
            FindBookmark = 1,
            Count = 2
        }
        public static int CurrentHotWnd { get; private set; }
        public static void IncrementHotWnd()
        {
            CurrentHotWnd++;
            if (CurrentHotWnd == (int)EHotWnd.Count)
            {
                CurrentHotWnd = 0;
            }
        }

        private bool _flip = false;

        public Dictionary<Type, Window> ChildWindows { get; set; }


        private void ToggleHotWnd(object sender, HotKeyEventArgs e)
        {
            CreateBookmarkWnd createBookmarkWnd = (CreateBookmarkWnd)ChildWindows[typeof(CreateBookmarkWnd)];
            FindBookmarkWnd findBookmarkWnd = (FindBookmarkWnd)ChildWindows[typeof(FindBookmarkWnd)];

            if (!createBookmarkWnd.IsVisible && !findBookmarkWnd.IsVisible)
            {
                CurrentHotWnd = (int)EHotWnd.CreateBookmark;
                _flip = false;
            }


            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                foreach (KeyValuePair<Type, Window> wnd in ChildWindows)
                {
                    if (wnd.Value.IsVisible)
                    {
                        wnd.Value.Hide();
                    }
                }
            }));

            string clipboardText = Env.GetClipboardText();

            Thread.Sleep(10);

            if (clipboardText != "" && !_flip)
            {
                CurrentHotWnd = (int)EHotWnd.CreateBookmark;
                _flip = true;
            }
            else
            {
                IncrementHotWnd();
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (CurrentHotWnd == (int)EHotWnd.CreateBookmark) createBookmarkWnd.Fire();
                else if (CurrentHotWnd == (int)EHotWnd.FindBookmark) findBookmarkWnd.Fire();
            }));
        }

        public async void ExecSignInAsync()
        {
            string outstr = await Env.FirebaseController.SignInUserAsync();
            if(outstr != "")
            {
                Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
            }
            else
            {
                Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.BookmarksView;
            }
        }

        public MainWindow()
        {

            Env.Initialize(this);
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.Loading;

            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
            MinimizeToTray.Enable(this);

            HotKeyManager.RegisterHotKey(Keys.None, KeyModifiers.Windows | KeyModifiers.Control);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(ToggleHotWnd);

            ChildWindows = new Dictionary<Type, Window>();
            ChildWindows[typeof(CreateBookmarkWnd)] = new CreateBookmarkWnd();
            ChildWindows[typeof(FindBookmarkWnd)] = new FindBookmarkWnd();


            ExecSignInAsync();

        }

        
    }
}

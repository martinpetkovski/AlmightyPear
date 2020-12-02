using AlmightyPear.Controller;
using AlmightyPear.Controls;
using AlmightyPear.Model;
using AlmightyPear.Utils;
using AlmightyPear.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = AlmightyPear.View.MessageBox;

namespace AlmightyPear
{
    public partial class MainWindow : MetroWindow
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

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

        public static bool IsApplicationActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        private void ToggleHotWnd(object sender, HotKeyEventArgs e)
        {
            CreateBookmarkWnd createBookmarkWnd = (CreateBookmarkWnd)ChildWindows[typeof(CreateBookmarkWnd)];
            FindBookmarkWnd findBookmarkWnd = (FindBookmarkWnd)ChildWindows[typeof(FindBookmarkWnd)];

            if ((!createBookmarkWnd.IsVisible && !findBookmarkWnd.IsVisible)
                || !IsApplicationActivated())
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

            if (clipboardText != "" && !_flip)
            {
                CurrentHotWnd = (int)EHotWnd.CreateBookmark;
                _flip = true;
            }
            else
            {
                IncrementHotWnd();
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (CurrentHotWnd == (int)EHotWnd.CreateBookmark) createBookmarkWnd.Fire();
                else if (CurrentHotWnd == (int)EHotWnd.FindBookmark) findBookmarkWnd.Fire();
            }));
        }

        public async Task OnSuccessfulSignInAsync()
        {
            await Env.FirebaseController.LoadCustomProfileDataAsync();
            ThemeManager.SetTheme(Env.UserData.CustomModel.Theme);
            Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.BookmarksView;
        }

        public async Task ExecSignInAsync()
        {
            string outstr = await Env.FirebaseController.SignInUserAsync();
            if (outstr != "")
            {
                Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
            }
            else
            {
                await OnSuccessfulSignInAsync();
            }

            if (Env.UserData.CustomModel.AnimationsLevel == 2) mah_contentControl.Reload();
        }

        private async Task InitializeThemesAsync()
        {
            await Env.FirebaseController.GetThemesAsync();
        }

        public async Task InitializeAsync()
        {
            Env.Initialize(this);
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.Loading;
            Style = (Style)FindResource(typeof(Window));
            await InitializeThemesAsync();
            InitializeComponent();

            MinimizeToTray.Enable(this);

            HotKeyManager.RegisterHotKey(Keys.None, KeyModifiers.Windows | KeyModifiers.Control);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(ToggleHotWnd);

            ChildWindows = new Dictionary<Type, Window>();
            ChildWindows[typeof(CreateBookmarkWnd)] = new CreateBookmarkWnd();
            ChildWindows[typeof(FindBookmarkWnd)] = new FindBookmarkWnd();

            await ExecSignInAsync();


        }

        public MainWindow()
        {
            InitializeAsync();
        }

        public void Mi_ClearTempBin_Click(object sender, RoutedEventArgs e)
        {
            Env.BinController.ClearTempBin();
        }

        public void Mi_LogOut_Click(object sender, RoutedEventArgs e)
        {
            Env.FirebaseController.LogOutUser();
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
        }

        private void Mi_Quit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Mi_minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void OnChangeWindowState()
        {
            if (mah_contentControl != null)
            {
                if (Env.UserData.CustomModel.AnimationsLevel == 2) mah_contentControl.Reload();
            }
        }
    }
}

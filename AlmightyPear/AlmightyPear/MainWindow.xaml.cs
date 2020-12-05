using AlmightyPear.Controller;
using AlmightyPear.Controls;
using AlmightyPear.Model;
using AlmightyPear.Utils;
using AlmightyPear.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;
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

        public Dictionary<Type, Window> ChildWindows { get; set; }
        public Dictionary<string, HotKeyManager> HotKeyManagers { get; set; }

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

        public async Task ClipboardChanged(string inputValue)
        {
            await Task.Factory.StartNew(() =>
            {
                string a = Env.GetClipboardText();
                while (inputValue == a)
                {
                    Thread.Sleep(10);
                    a = Env.GetClipboardText();
                }
            });
        }

        private string _prevClipboardText = "";

        private async void ShowHotWndCreateBookmarkAsync(object sender, HotKeyEventArgs e)
        {
            CreateBookmarkWnd createBookmarkWnd = (CreateBookmarkWnd)ChildWindows[typeof(CreateBookmarkWnd)];

            if(createBookmarkWnd.IsVisible)
            {
                Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() =>
                {
                    createBookmarkWnd.Hide();
                }));
            }

            InputSimulator inputSim = new InputSimulator();
            string clipboardText = Env.GetClipboardText();
            inputSim.Keyboard.KeyUp(VirtualKeyCode.LWIN);
            inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

            if (clipboardText != "" && clipboardText != _prevClipboardText)
            {
                await ClipboardChanged(clipboardText);
            }

            _prevClipboardText = Env.GetClipboardText();

            Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() =>
            {
                createBookmarkWnd.Fire();
            }));
        }

        private void ShowHotWndFindBookmark(object sender, HotKeyEventArgs e)
        {
            FindBookmarkWnd findBookmarkWnd = (FindBookmarkWnd)ChildWindows[typeof(FindBookmarkWnd)];
            if (!findBookmarkWnd.IsVisible)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    findBookmarkWnd.Fire();
                }));
            }
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

            MinimizeToTray.Register(this);

            HotKeyManagers = new Dictionary<string, HotKeyManager>();

            HotKeyManagers["win+alt"] = new HotKeyManager();
            HotKeyManagers["win+alt"].RegisterHotKey(Keys.None, KeyModifiers.Windows | KeyModifiers.Alt);
            HotKeyManagers["win+alt"].HotKeyPressed += new EventHandler<HotKeyEventArgs>(ShowHotWndFindBookmark);

            HotKeyManagers["win+ctrl"] = new HotKeyManager();
            HotKeyManagers["win+ctrl"].RegisterHotKey(Keys.None, KeyModifiers.Windows | KeyModifiers.Control);
            HotKeyManagers["win+ctrl"].HotKeyPressed += new EventHandler<HotKeyEventArgs>(ShowHotWndCreateBookmarkAsync);

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

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            System.Windows.Application.Current.Shutdown();
        }
    }
}

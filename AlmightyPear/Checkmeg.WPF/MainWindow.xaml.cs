using Checkmeg.WPF.Controller;
using Checkmeg.WPF.Model;
using Checkmeg.WPF.Utils;
using Checkmeg.WPF.View;
using Core;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace Checkmeg.WPF
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
                string a = ClipboardManager.GetClipboardText();
                while (inputValue == a)
                {
                    Thread.Sleep(10);
                    a = ClipboardManager.GetClipboardText();
                }
            });
        }

        private string _prevClipboardText = "";

        public async void ShowHotWndCreateBookmarkAsync(object sender, HotKeyEventArgs e)
        {
            CreateBookmarkWnd createBookmarkWnd = (CreateBookmarkWnd)ChildWindows[typeof(CreateBookmarkWnd)];

            if (createBookmarkWnd.IsVisible)
            {
                Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() =>
                {
                    createBookmarkWnd.Hide();
                }));
            }

            InputSimulator inputSim = new InputSimulator();
            string clipboardText = ClipboardManager.GetClipboardText();
            inputSim.Keyboard.KeyUp(VirtualKeyCode.LWIN);
            inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

            if (clipboardText != "" && clipboardText != _prevClipboardText)
            {
                await ClipboardChanged(clipboardText);
            }

            _prevClipboardText = ClipboardManager.GetClipboardText();

            Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() =>
            {
                createBookmarkWnd.Fire("", e.AdditionalPath);
            }));
        }

        public void ShowHotWndFindBookmark(object sender, HotKeyEventArgs e)
        {
            FindBookmarkWnd findBookmarkWnd = (FindBookmarkWnd)ChildWindows[typeof(FindBookmarkWnd)];
            if (!findBookmarkWnd.IsVisible)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    findBookmarkWnd.Fire(Path.GetFileNameWithoutExtension(e.AdditionalPath));
                }));
            }
        }

        public async Task OnSuccessfulSignInAsync()
        {
            await Engine.Env.FirebaseController.LoadCustomProfileDataAsync();
            Engine.Env.ThemeManager.SetTheme(Engine.Env.UserData.CustomModel.Theme);
            Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.BookmarksView;
        }

        public async Task ExecSignInAsync()
        {
            string outstr = await Engine.Env.FirebaseController.SignInUserAsync();
            if (outstr != "")
            {
                Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
            }
            else
            {
                await OnSuccessfulSignInAsync();
            }

            if (Engine.Env.UserData.CustomModel.AnimationsLevel == 2) mah_contentControl.Reload();
        }

        private async Task InitializeThemesAsync()
        {
            await Engine.Env.FirebaseController.GetThemesAsync();
        }

        public async Task InitializeAsync()
        {
            Env.Initialize(this);
            Engine.Env.Initialize();
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.Loading;
            Style = (Style)FindResource(typeof(Window));
            await InitializeThemesAsync();
            InitializeComponent();

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
            WindowState = Env.StartupState;
        }

        private void ProcessOutput(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }

        public MainWindow()
        {
            InitializeAsync();
        }

        public void Mi_ClearTempBin_Click(object sender, RoutedEventArgs e)
        {
            Engine.Env.BinController.ClearTempBin();
        }

        public void Mi_LogOut_Click(object sender, RoutedEventArgs e)
        {
            Engine.Env.FirebaseController.LogOutUser();
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
                if (Engine.Env.UserData.CustomModel.AnimationsLevel == 2) mah_contentControl.Reload();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            Show();
            Activate();
            Focus();
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void RootWnd_StateChanged(object sender, EventArgs e)
        {
            var minimized = (WindowState == WindowState.Minimized);
            ShowInTaskbar = !minimized;
        }

        private void Mi_TrayQuit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Mi_TraySignOut_Click(object sender, RoutedEventArgs e)
        {
            Engine.Env.FirebaseController.LogOutUser();
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
        }

        private void Mi_ShowCheckmeg_Click(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void Mi_TrayFind_Click(object sender, RoutedEventArgs e)
        {
            ShowHotWndFindBookmark(sender, null);
        }

        private void Mi_TrayCreate_Click(object sender, RoutedEventArgs e)
        {
            ShowHotWndCreateBookmarkAsync(sender, null);
        }

        private void RootWnd_Closed(object sender, EventArgs e)
        {
        }
    }
}

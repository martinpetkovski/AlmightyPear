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
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
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

        private async void Mi_SaveAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (Env.BinController.HasEditedBookmarks() || Env.BinData.HasCreatedBookmarks)
            {
                string pluralToken = Env.BinController.EditedBookmarks.Count == 1 ? "" : "s";
                string token = Env.BinController.EditedBookmarks.Count + " edited bookmark" + pluralToken + " and the newly created bookmark";
                if (!Env.BinController.HasEditedBookmarks() && Env.BinData.HasCreatedBookmarks)
                {
                    token = "the newly created bookmark";
                }
                else if (Env.BinController.HasEditedBookmarks() && !Env.BinData.HasCreatedBookmarks)
                {
                    token = Env.BinController.EditedBookmarks.Count + " edited bookmark" + pluralToken;
                }

                int result = await MessageBox.FireAsync("Save All",
                    "You are about to save " + token + "." +
                    "\nAre you sure you want to proceed?",
                    new List<string>() { "Yes", "No" });

                if (result == 0)
                {
                    Env.ClearClipboard();
                    ProgressBarWnd.FireAsync();
                    await Env.BinController.SaveEditedBookmarksAsync(ProgressBarWnd.Instance, ProgressBarWnd.UpdateProgress);
                    object mainViewObj = this.FindName("ctrl_mainView");
                    if (mainViewObj != null)
                    {
                        if (mainViewObj is MainViewControl)
                        {
                            await ((MainViewControl)mainViewObj).ctrl_CreateBookmark.CreateAsync();
                        }
                    }
                }
            }
            else
            {
                await MessageBox.FireAsync("Save All",
                   "Nothing to save.",
                   new List<string>() { "Ok" });
            }
        }

        private void Mi_ClearTempBin_Click(object sender, RoutedEventArgs e)
        {
            Env.BinController.DeleteBin(Env.BinController.GetBin(Env.TempBinPath));
        }

        private void Mi_LogOut_Click(object sender, RoutedEventArgs e)
        {
            Env.FirebaseController.LogOutUser();
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
        }
    }
}

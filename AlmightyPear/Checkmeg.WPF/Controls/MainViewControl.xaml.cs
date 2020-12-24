using Checkmeg.WPF.Controller;
using Checkmeg.WPF.Model;
using Checkmeg.WPF.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Checkmeg.WPF.Controls
{
    public partial class MainViewControl : UserControl
    {
        private MainViewModel _mainViewData;

        protected class MainViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected MainViewModel MainViewData { get => _mainViewData; set => _mainViewData = value; }

        public MainViewControl()
        {
            InitializeComponent();
        }

        private void Btn_updateProfile_Click(object sender, RoutedEventArgs e)
        {
            Engine.Env.FirebaseController.UpdateProfileAsync(tb_displayName.Text, "");
        }

        private void Btn_clearTempBin_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindow.Mi_ClearTempBin_Click(sender, e);
        }

        private void Btn_logOut_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindow.Mi_LogOut_Click(sender, e);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Engine.Env.UserData.CustomModel.AnimationsLevel >= 1)
            {
                mah_bookmarksViewContentControl.TransitionsEnabled = true;
                mah_createBookmarkContentControl.TransitionsEnabled = true;
            }
            else
            {
                mah_bookmarksViewContentControl.TransitionsEnabled = false;
                mah_createBookmarkContentControl.TransitionsEnabled = false;
            }

            mah_bookmarksViewContentControl.Reload();
            mah_createBookmarkContentControl.Reload();

        }

        public void StartWscProc(string arg)
        {
            Process proc = new Process();
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                proc.StartInfo.Verb = "runas";
            }
            proc.StartInfo.FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + Env.CheckmegWSCEXE;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Arguments = arg;
            proc.Start();
        }

        private void Btn_wndContextMenu_Click(object sender, RoutedEventArgs e)
        {
            StartWscProc("-c");
        }

        private void Btn_wndContextMenuRemove_Click(object sender, RoutedEventArgs e)
        {
            StartWscProc("-d");
        }

        private void Btn_wndAddToStartup_Click(object sender, RoutedEventArgs e)
        {
            StartWscProc("-s|1|" + System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void Btn_wndRemoveFromStartup_Click(object sender, RoutedEventArgs e)
        {
            StartWscProc("-s|0|" + System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}

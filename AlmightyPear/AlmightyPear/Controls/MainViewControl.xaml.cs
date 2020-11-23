using AlmightyPear.Controller;
using AlmightyPear.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace AlmightyPear.Controls
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

        private void Btn_LogOut_Click(object sender, RoutedEventArgs e)
        {
            Env.FirebaseController.LogOutUser();
            Env.MainWindowData.WindowState = MainWindowModel.EMainWindowState.SignIn;
        }

        private async void Btn_SaveAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            Env.ClearClipboard();
            Env.BinController.SaveEditedBookmarksAsync();
            await ctrl_CreateBookmark.CreateAsync();
        }
    }
}

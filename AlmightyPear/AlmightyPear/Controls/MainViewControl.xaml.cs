using AlmightyPear.Controller;
using AlmightyPear.Model;
using System.Windows;
using System.Windows.Controls;

namespace AlmightyPear.Controls
{
    public partial class MainViewControl : UserControl
    {
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

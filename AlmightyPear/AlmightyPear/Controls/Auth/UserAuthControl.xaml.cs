using Checkmeg.WPF.Controller;
using Checkmeg.WPF.Utils;
using Firebase.Auth;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Checkmeg.WPF.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserAuthControl : UserControl
    {
        public UserAuthControl()
        {
            InitializeComponent();
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private async void Btn_signin_ClickAsync(object sender, RoutedEventArgs e)
        {
            Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.Loading;
            string msg = await Engine.Env.FirebaseController.SignInUserAsync(tb_email.Text, tb_password.Password, cb_RememberMe.IsChecked.Value);
            if (!cb_RememberMe.IsChecked.Value)
                Engine.Env.FirebaseController.DeleteToken();

            if (msg != "")
            {
                Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.SignIn;
                txt_messagebox.Text = msg;
            }
            else
            {
                await Env.MainWindow.OnSuccessfulSignInAsync();
            }
        }

        private void Tb_email_KeyDown(object sender, KeyEventArgs e)
        {
            txt_messagebox.Text = "";
        }

        private void Tb_password_KeyDown(object sender, KeyEventArgs e)
        {
            txt_messagebox.Text = "";
        }

        private void Btn_passwordReset_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.ChangePw;
        }

        private void Btn_quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

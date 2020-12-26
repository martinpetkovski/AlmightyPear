using Checkmeg.WPF.Controller;
using System.Windows;
using System.Windows.Controls;
using Checkmeg.WPF.View;
using MessageBox = Checkmeg.WPF.View.MessageBox;
using Checkmeg.WPF.Utils;

namespace Checkmeg.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ResetPasswordControl.xaml
    /// </summary>
    public partial class ResetPasswordControl : UserControl
    {

        public ResetPasswordControl()
        {
            InitializeComponent();
        }

        private async void Btn_Submit_ClickAsync(object sender, RoutedEventArgs e)
        {
            string email;
            if (Engine.Env.UserData.Email != "")
            {
                email = Engine.Env.UserData.Email;
            }
            else
            {
                email = tb_email.Text;
            }

            Engine.FirebaseController.SChangePasswordResult result = await Engine.Env.FirebaseController.ChangePasswordAsync(email, tb_oldPassword.Password, tb_newPassword.Password, tb_repeatPassword.Password);
            tb_newPassword.Password = "";
            tb_oldPassword.Password = "";
            tb_repeatPassword.Password = "";

            await MessageBox.FireAsync(TranslationSource.Instance["PasswordReset"], result.message, new System.Collections.Generic.List<string>() { "Ok" });
            if(result.success && !Engine.Env.UserData.IsLoggedIn)
            {
                Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.SignIn;
            }

        }

        private void Btn_return_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindowData.WindowState = Model.MainWindowModel.EMainWindowState.SignIn;
        }
    }
}

using AlmightyPear.Controller;
using System.Windows;
using System.Windows.Controls;
using AlmightyPear.View;
using MessageBox = AlmightyPear.View.MessageBox;

namespace AlmightyPear.Controls
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
            if (Env.UserData.Email != "")
            {
                email = Env.UserData.Email;
            }
            else
            {
                email = tb_email.Text;
            }

            FirebaseController.SChangePasswordResult result = await Env.FirebaseController.ChangePasswordAsync(email, tb_oldPassword.Password, tb_newPassword.Password, tb_repeatPassword.Password);
            tb_newPassword.Password = "";
            tb_oldPassword.Password = "";
            tb_repeatPassword.Password = "";

            await MessageBox.FireAsync("Change Password", result.message, new System.Collections.Generic.List<string>() { "Ok" });
            if(result.success && !Env.UserData.IsLoggedIn)
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

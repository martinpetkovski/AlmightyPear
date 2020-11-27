using AlmightyPear.Controller;
using AlmightyPear.Model;
using AlmightyPear.Utils;
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

        private void Btn_updateProfile_Click(object sender, RoutedEventArgs e)
        {
            Env.FirebaseController.UpdateProfileAsync(tb_displayName.Text, "");
        }

        private void Btn_saveAll_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindow.Mi_SaveAll_ClickAsync(sender, e);
        }

        private void Btn_fullReload_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindow.Mi_FullReload_ClickAsync(sender, e);
        }

        private void Btn_clearTempBin_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindow.Mi_ClearTempBin_Click(sender, e);
        }

        private void Btn_logOut_Click(object sender, RoutedEventArgs e)
        {
            Env.MainWindow.Mi_LogOut_Click(sender, e);
        }

        private void Cb_themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeManager.SetTheme((ThemeManager.Theme)(e.AddedItems[0]));
        }
    }
}

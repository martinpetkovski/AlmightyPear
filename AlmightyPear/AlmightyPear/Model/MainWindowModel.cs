using AlmightyPear.Controller;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlmightyPear.Model
{
    class MainWindowModel : INotifyPropertyChanged
    {
        public enum EMainWindowState
        {
            SignIn,
            Register,
            ForgotPw,
            BookmarksView,
            Loading,
            Count
        }

        private EMainWindowState _windowState;
        public EMainWindowState WindowState
        {
            get
            {
                return _windowState;
            }
            set
            {
                _windowState = value;
                OnPropertyChanged();
                Env.MainWindow.OnChangeWindowState();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

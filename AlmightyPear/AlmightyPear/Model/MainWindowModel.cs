using Checkmeg.WPF.Controller;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Checkmeg.WPF.Model
{
    class MainWindowModel : INotifyPropertyChanged
    {
        public enum EMainWindowState
        {
            SignIn,
            Register,
            ChangePw,
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

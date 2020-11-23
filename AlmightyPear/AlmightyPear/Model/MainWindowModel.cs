using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

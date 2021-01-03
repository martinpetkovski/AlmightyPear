using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Checkmeg.WPF.Model
{
    class EnvModel : INotifyPropertyChanged
    {
        private string _filter = "";
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
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

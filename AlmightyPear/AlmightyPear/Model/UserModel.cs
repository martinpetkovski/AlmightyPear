using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AlmightyPear.Model
{
    class UserModel : INotifyPropertyChanged
    {
        public void Initialize(string id, string email)
        {
            Email = email;
            ID = id;
            IsLoggedIn = true;
        }

        public void Deinitialize()
        {
            Email = "";
            ID = "";
            if (Bookmarks != null)
            {
                Bookmarks.Clear();
                Bookmarks = null;
            }
            IsLoggedIn = false;
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            private set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string _id;
        public string ID
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoggedIn = false;
        public bool IsLoggedIn
        {
            get
            {
                return _isLoggedIn;
            }
            private set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, BookmarkModel> _bookmarks;
        public Dictionary<string, BookmarkModel> Bookmarks
        {
            get
            {
                return _bookmarks;
            }
            set
            {
                _bookmarks = value;
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

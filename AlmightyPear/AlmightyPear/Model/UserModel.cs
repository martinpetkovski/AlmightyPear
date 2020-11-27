using AlmightyPear.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AlmightyPear.Model
{
    class UserModel : INotifyPropertyChanged
    {
        public void Initialize(string id, string email, string displayName, string photoUrl)
        {
            Email = email;
            ID = id;
            DisplayName = displayName;
            PhotoUrl = photoUrl;
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
                OnPropertyChanged("IsLoggedIn");
            }
        }

        private string _displayName = "";
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        private string _photoUrl = "https://static3.depositphotos.com/1000635/120/i/600/depositphotos_1208368-stock-photo-white-paper-seamless-background.jpg";
        public string PhotoUrl
        {
            get
            {
                return _photoUrl;
            }
            set
            {
                _photoUrl = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return ID != "";
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

        private Dictionary<string, ThemeManager.Theme> _themes;
        public Dictionary<string, ThemeManager.Theme> Themes
        {
            get
            {
                if (_themes == null)
                    _themes = new Dictionary<string, ThemeManager.Theme>();
                return _themes;
            }
            set
            {
                _themes = value;
                OnPropertyChanged();
                OnPropertyChanged("ThemesList");
            }
        }

        public List<ThemeManager.Theme> ThemesList
        {
            get
            {
                return Themes.Values.ToList();
            }
        }

        public void AddTheme(ThemeManager.Theme theme)
        {
            Themes.Add(theme.Name, theme);
            OnPropertyChanged("Themes");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

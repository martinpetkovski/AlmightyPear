using Engine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Engine
{
    public class UserModel : INotifyPropertyChanged
    {
        public class CustomUserModel : INotifyPropertyChanged
        {
            public CustomUserModel()
            {
                Theme = "BasicDark";
                AnimationsLevel = 2;
            }

            private string _theme;
            public string Theme
            {
                get
                {
                    return _theme;
                }
                set
                {
                    _theme = value;
                    Env.ThemeManager.SetTheme(value);
                    OnPropertyChanged();
                }
            }

            private int _animationsLevel;
            public int AnimationsLevel
            {
                get
                {
                    return _animationsLevel;
                }
                set
                {
                    _animationsLevel = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }


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
            CustomModel.Theme = "BasicDark";
            CustomModel.AnimationsLevel = 2;
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

        private CustomUserModel _customModel;
        public CustomUserModel CustomModel
        {
            get
            {
                if (_customModel == null)
                    _customModel = new CustomUserModel();
                return _customModel;
            }
            set
            {
                _customModel = value;
                OnPropertyChanged();
            }
        }


        private Dictionary<string, BookmarkModel> _bookmarks;
        public Dictionary<string, BookmarkModel> Bookmarks
        {
            get
            {
                if (_bookmarks == null)
                    _bookmarks = new Dictionary<string, BookmarkModel>();
                return _bookmarks;
            }
            set
            {
                _bookmarks = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, ThemeModel> _themes;
        public Dictionary<string, ThemeModel> Themes
        {
            get
            {
                if (_themes == null)
                    _themes = new Dictionary<string, ThemeModel>();
                return _themes;
            }
            set
            {
                _themes = value;
                OnPropertyChanged();
                OnPropertyChanged("ThemesList");
            }
        }

        public List<ThemeModel> ThemesList
        {
            get
            {
                return Themes.Values.ToList();
            }
        }

        public void AddTheme(ThemeModel theme)
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

using AlmightyPear.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AlmightyPear.Model
{
    public class BinMetaModel : INotifyPropertyChanged
    {
        private BinModel _rootBin;
        public BinModel RootBin
        {
            get
            {
                if(_rootBin == null)
                    _rootBin = new BinModel("root", "");
                return _rootBin;
            }
            set
            {
                _rootBin = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<int, HashSet<string>> _binsByDepth;
        public Dictionary<int, HashSet<string>> BinsByDepth
        {
            get
            {
                return _binsByDepth;
            }
        }

        public void AddBinByDepth(string name, int depth)
        {
            if(!BinsByDepth.ContainsKey(depth))
            {
                BinsByDepth[depth] = new HashSet<string>();
            }

            BinsByDepth[depth].Add(name);
            OnPropertyChanged("BinsByDepth");
        }
        
        public BinMetaModel()
        {
            _binsByDepth = new Dictionary<int, HashSet<string>>();
        }

        public void Deinitialize()
        {
            RootBin = null;
        }

        public string BookmarksViewCaption
        {
            get
            {
                if (Env.BinController.HasEditedBookmarks())
                    return "Bookmarks View (*)";
                else
                    return "Bookmarks View";
            }
            set
            {
                OnPropertyChanged();
            }
        }

        bool _hasCreatedBookmarks;
        public string BookmarksCreateCaption
        {
            get
            {
                if (_hasCreatedBookmarks)
                    return "Create Bookmark (*)";
                else
                    return "Create Bookmark";
            }
            set
            {
                _hasCreatedBookmarks = bool.Parse(value);
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
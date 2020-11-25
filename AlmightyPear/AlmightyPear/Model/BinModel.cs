using AlmightyPear.Controller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlmightyPear.Model
{
    public class BinModel : IBinItem, INotifyPropertyChanged
    {
        private Dictionary<string, IBinItem> _childBinItems;
        public Dictionary<string, IBinItem> BinItems
        {
            get
            {
                return _childBinItems;
            }
            set
            {
                _childBinItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IBinItem> BinItemsCollection
        {
            get
            {
                return new ObservableCollection<IBinItem>(_childBinItems.Values);
            }
        }

        public void AddItemToBin(string key, IBinItem value)
        {
            IBinItem nextBin;
            lock (BinItems)
            {
                bool exists = BinItems.TryGetValue(key, out nextBin);
                if (exists && nextBin is BinModel)
                {
                    BinModel nextBinModel = (BinModel)nextBin;

                    foreach (KeyValuePair<string, IBinItem> binItem in nextBinModel.BinItems)
                    {
                        nextBinModel.AddItemToBin(binItem.Key, binItem.Value);
                    }
                }
                else
                {
                    _childBinItems.Add(key, value);
                    OnPropertyChanged("BinItems");
                    OnPropertyChanged("BinItemsCollection");
                }
            }

        }

        public void Nuke()
        {
            _childBinItems.Clear();
            OnPropertyChanged("BinItems");
            OnPropertyChanged("BinItemsCollection");
        }


        public static int FilterPath(string path, string filter, char separator)
        {
            string[] filterTokens = filter.Split(separator);
            string[] pathTokens = path.Split(Env.PathSeparator);
            int maxDepth = Math.Min(filterTokens.Length, pathTokens.Length);
            int totalDepth = Math.Max(filterTokens.Length, pathTokens.Length);

            int matchDepth = 0;
            int totalMatchDepth = 0;
            for (int i = 0; i < pathTokens.Length; i++)
            {
                if (matchDepth >= filterTokens.Length)
                    break;

                if (pathTokens[i].Trim(' ').ToLower() == filterTokens[matchDepth].Trim(' ').ToLower())
                {
                    matchDepth++;
                    totalMatchDepth = Math.Max(totalMatchDepth, matchDepth);
                }
                else
                {
                    matchDepth = 0;
                }
            }

            return totalMatchDepth == filterTokens.Length ? 100 : 0;

        }

        public int FilterScore(FilterToken filter, ref Dictionary<string, int> pathScores)
        {
            int existScore = 0;
            if (pathScores.TryGetValue(Path, out existScore))
            {
                return existScore;
            }

            string filterString = filter.ToString();
            if (filterString == "")
                return 100;

            int score = 0;

            if (filter.TokenType == FilterToken.FilterTokenType.Path)
            {
                score = BinModel.FilterPath(Path, filterString, Env.PathSeparator);
            }

            if (filter.TokenType == FilterToken.FilterTokenType.Any)
            {
                score = BinModel.FilterPath(Path, filterString, Env.PathSeparator);
                score = Math.Max(score, BinModel.FilterPath(Path, filterString, ' '));
            }

            foreach (var binItem in BinItems)
            {
                if (binItem.Value is BookmarkModel)
                {
                    score = Math.Max(score, ((BookmarkModel)binItem.Value).FilterScore(filter, ref pathScores));
                }
                else if (binItem.Value is BinModel)
                {
                    score = Math.Max(score, ((BinModel)binItem.Value).FilterScore(filter, ref pathScores));
                }
            };

            pathScores.Add(Path, score);
            return score;
        }

        public async void RecalculatePathsAsync()
        {
            foreach (KeyValuePair<string, IBinItem> binItem in BinItems)
            {
                if (binItem.Value is BinModel)
                {
                    BinModel model = ((BinModel)binItem.Value);
                    model.Path = Path + Env.PathSeparator + model.Name;
                    model.RecalculatePathsAsync();
                }
                else if (binItem.Value is BookmarkModel)
                {
                    BookmarkModel model = ((BookmarkModel)binItem.Value);
                    model.Path = Path;
                    await Env.FirebaseController.UpdateBookmarkAsync(model);
                }
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _path;
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        public int PathDepth
        {
            get
            {
                return Path.Split(Env.PathSeparator).Length - 1;
            }
        }

        public BinModel(string name, string path)
        {
            _childBinItems = new Dictionary<string, IBinItem>();
            Name = name;
            Path = path;

            Env.BinData.AddBinByDepth(path, Path.Split(Env.PathSeparator).Length);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

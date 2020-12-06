using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    public class BinController
    {
        private Dictionary<string, BookmarkModel> _editedBookmarks;
        public Dictionary<string, BookmarkModel> EditedBookmarks
        {
            get
            {
                if (_editedBookmarks == null)
                    _editedBookmarks = new Dictionary<string, BookmarkModel>();

                return _editedBookmarks;
            }
        }


        public BinController()
        {
        }

        public void Deinitalize()
        {
            Env.BinData.Deinitialize();
        }

        public static List<string> GetBinsFromPath(string path)
        {
            return path.Split(Env.PathSeparator).ToList();
        }

        public void AddBookmark(BookmarkModel bookmark)
        {
            string binPath = bookmark.Path;
            List<string> bins = GetBinsFromPath(binPath);

            string path = "";
            BinModel parentBin = Env.BinData.RootBin;
            int i = 0;
            foreach (string bin in bins)
            {
                if (i == 0) path += bin;
                else path += Env.PathSeparator + bin;

                IBinItem nextBin = null;

                lock (parentBin.BinItems)
                {
                    bool exists = parentBin.BinItems.TryGetValue(bin, out nextBin);
                    if (!exists)
                    {
                        nextBin = new BinModel(bin, path);
                        parentBin.AddItemToBin(bin, nextBin);
                    }
                }

                BinModel nextBinModel = (BinModel)nextBin;
                if (nextBinModel != null)
                {
                    parentBin = nextBinModel;
                }
                i++;
            }
            parentBin.AddItemToBin(bookmark.ID, bookmark);
        }

        public void GenerateBinTree()
        {
            if (Env.UserData.Bookmarks == null) return;
            Env.BinData.BinsByDepth.Clear();
            Env.BinData.RootBin = new BinModel("root", "");

            Parallel.ForEach(Env.UserData.Bookmarks, (bookmark) =>
            {
                AddBookmark(bookmark.Value);
            });
        }

        public void DeleteBookmark(string ID)
        {
            Env.UserData.Bookmarks.Remove(ID);
            GenerateBinTree();
        }

        public void DeleteBookmarksFromBin(BinModel bin)
        {
            foreach (KeyValuePair<string, IBinItem> item in bin.BinItems)
            {
                if (item.Value is BinModel)
                {
                    DeleteBookmarksFromBin((BinModel)item.Value);
                }
                else if (item.Value is BookmarkModel)
                {
                    BookmarkModel bookmark = (BookmarkModel)item.Value;
                    Env.FirebaseController.DeleteBookmark(bookmark);
                }
            }
        }

        public bool PurgeBinTree(BinModel parentBin)
        {
            bool shouldNuke = true;
            foreach (KeyValuePair<string, IBinItem> binItem in parentBin.BinItems)
            {
                if (binItem.Value is BookmarkModel)
                {
                    shouldNuke = false;
                    break;
                }
                else if (binItem.Value is BinModel)
                {
                    shouldNuke = PurgeBinTree((BinModel)binItem.Value);
                    if (!shouldNuke)
                        break;
                }
            }

            if (shouldNuke)
                parentBin.Nuke();

            return shouldNuke;

        }

        public void DeleteBin(BinModel item)
        {
            Dictionary<string, IBinItem> binItems = GetItemsAndChildren(item);
            if (binItems != null)
            {
                foreach (KeyValuePair<string, IBinItem> binItem in binItems)
                {
                    if (binItem.Value is BookmarkModel)
                    {
                        Env.FirebaseController.DeleteBookmark((BookmarkModel)binItem.Value);
                    }
                }

                GenerateBinTree();
            }
        }

        public void ChangePath(IBinItem item, string path)
        {
            item.Path = path;

            if (item is BinModel)
            {
                BinModel model = (BinModel)item;
                model.Name = model.Path.Substring(model.Path.LastIndexOf(Env.PathSeparator) + 1, model.Path.Length - model.Path.LastIndexOf(':') - 1);
                model.RecalculatePaths();
            }

            GenerateBinTree();
        }

        public BinModel GetBin(string path)
        {
            BinModel retVal = null;
            List<string> bins = GetBinsFromPath(path);

            bool found = false;
            BinModel currentBin = Env.BinData.RootBin;
            foreach (string currentBinName in bins)
            {
                IBinItem binItem;
                found = currentBin.BinItems.TryGetValue(currentBinName, out binItem);
                if (found)
                {
                    currentBin = (BinModel)binItem;
                    if (currentBin == null)
                    {
                        found = false;
                    }
                }

                if (!found)
                    break;
            }

            if (found)
            {
                retVal = currentBin;
            }

            return retVal;
        }

        public Dictionary<string, IBinItem> GetBinItems(string path)
        {
            BinModel bin = GetBin(path);
            if (bin != null)
            {
                return bin.BinItems;
            }

            return null;
        }

        public Dictionary<string, IBinItem> GetItemsAndChildren(BinModel item)
        {
            Dictionary<string, IBinItem> retVal = null;
            if (item != null)
            {
                retVal = item.BinItems;
                foreach (IBinItem binModel in item.BinItemsCollection)
                {
                    if (binModel is BinModel)
                    {
                        retVal = retVal.Concat(GetItemsAndChildren((BinModel)binModel)).ToDictionary(p => p.Key, p => p.Value);
                    }
                }
            }
            return retVal;
        }

        public void ClearTempBin()
        {
            DeleteBin(GetBin(Env.TempBinPath));
        }

    }
}

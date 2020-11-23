using AlmightyPear.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmightyPear.Controller
{
    class BinController
    {
        private Dictionary<string, BookmarkModel> EditedBookmarks { get; set; }


        public BinController()
        {
            EditedBookmarks = new Dictionary<string, BookmarkModel>();
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
                bool exists = parentBin.BinItems.TryGetValue(bin, out nextBin);
                if (!exists)
                {
                    nextBin = new BinModel(bin, path);
                    parentBin.AddItemToBin(bin, nextBin);
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

            foreach (KeyValuePair<string, BookmarkModel> bookmark in Env.UserData.Bookmarks)
            {
                AddBookmark(bookmark.Value);
            }
        }

        public void DeleteBookmark(BookmarkModel bookmark)
        {
            Env.UserData.Bookmarks.Remove(bookmark.ID);
            Env.FirebaseController.DeleteBookmark(bookmark);
            GenerateBinTree();
        }

        public void DeleteBookmarksFromBin(BinModel bin)
        {
            foreach(KeyValuePair<string, IBinItem> item in bin.BinItems)
            {
                if(item.Value is BinModel)
                {
                    DeleteBookmarksFromBin((BinModel)item.Value);
                }
                else if(item.Value is BookmarkModel)
                {
                    BookmarkModel bookmark = (BookmarkModel)item.Value;
                    DeleteBookmark(bookmark);
                }
            }
        }

        public bool PurgeBinTree(BinModel parentBin)
        {
            bool shouldNuke = true;
            foreach(KeyValuePair<string,IBinItem> binItem in parentBin.BinItems)
            {
                if(binItem.Value is BookmarkModel)
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
            List<string> bins = GetBinsFromPath(item.Path);

            BinModel parentBin = Env.BinData.RootBin;

            foreach (string bin in bins)
            {
                IBinItem nextBin = null;
                bool exists = parentBin.BinItems.TryGetValue(bin, out nextBin);
                if (exists)
                {
                    if (bin == bins.Last())
                    {
                        DeleteBookmarksFromBin((BinModel)nextBin);
                        parentBin.RemoveItemFromBin(bin);
                        break;
                    }
                    parentBin = (BinModel)nextBin;
                }
            }

            GenerateBinTree();
        }

        public void ChangePath(IBinItem item, string path)
        {
            item.Path = path;

            if (item is BinModel)
            {
                BinModel model = (BinModel)item;
                model.Name = model.Path.Substring(model.Path.LastIndexOf(Env.PathSeparator) + 1, model.Path.Length - model.Path.LastIndexOf(':') - 1);
                model.RecalculatePathsAsync();
            }
            else if(item is BookmarkModel)
            {
                MarkBookmarkForEdit((BookmarkModel)item);
            }

            GenerateBinTree();
        }

        public Dictionary<string, IBinItem> GetItems(string path)
        {
            Dictionary<string, IBinItem> retVal = null;
            List<string> bins = GetBinsFromPath(path);

            bool found = false;
            BinModel currentBin = Env.BinData.RootBin;
            foreach(string currentBinName in bins)
            {
                IBinItem binItem;
                found = currentBin.BinItems.TryGetValue(currentBinName, out binItem);
                if(found)
                {
                    currentBin = (BinModel)binItem;
                    if(currentBin == null)
                    {
                        found = false;
                    }
                }

                if (!found)
                    break;
            }

            if(found)
            {
                retVal = currentBin.BinItems;
            }

            return retVal;
        }

        public void MarkBookmarkForEdit(BookmarkModel bookmark)
        {
            if (!EditedBookmarks.ContainsKey(bookmark.ID))
            {
                EditedBookmarks.Add(bookmark.ID, bookmark);
            }
        }
        
        public async void SaveEditedBookmarksAsync()
        {
            foreach(KeyValuePair<string, BookmarkModel> bookmark in EditedBookmarks)
            {
                await Env.FirebaseController.UpdateBookmarkAsync(bookmark.Value);
            }

            EditedBookmarks.Clear();
        }

    }
}

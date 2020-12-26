using Checkmeg.WPF.Controller;
using Checkmeg.WPF.Model;
using Checkmeg.WPF.Utils;
using Checkmeg.WPF.View;
using Core;
using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Env = Checkmeg.WPF.Controller.Env;

namespace Checkmeg.WPF.Controls
{


    public partial class BookmarksTreeViewControl : UserControl
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public class BookmarksTreeViewControlModel : INotifyPropertyChanged
        {
            private void UpdateSelection()
            {
                OnPropertyChanged("SelectedBinItems");
                OnPropertyChanged("HasSelected");
                OnPropertyChanged("HasOneSelected");
                OnPropertyChanged("HasOnlyBinSelected");
                OnPropertyChanged("HasOnlyBookmarkSelected");
                OnPropertyChanged("HasOnlyOneBinSelected");
                OnPropertyChanged("HasOnlyOneBookmarkSelected");
                OnPropertyChanged("ContextedItem");
                OnPropertyChanged("IsContextedItemBin");
                OnPropertyChanged("IsContextedItemBookmark");
                OnPropertyChanged("IsContextedItemSelected");
                OnPropertyChanged("IsContextedItemNotSelected");
                OnPropertyChanged("IsContextedItemNotInArchive");
            }

            private IBinItem _contextedItem;
            public IBinItem ContextedItem
            {
                get
                {
                    return _contextedItem;
                }
                set
                {
                    _contextedItem = value;
                    UpdateSelection();
                }
            }

            public bool IsContextedItemBin
            {
                get
                {
                    return _contextedItem is BinModel;
                }
            }

            public bool IsContextedItemBookmark
            {
                get
                {
                    return _contextedItem is BookmarkModel;
                }
            }

            public bool IsContextedItemAny
            {
                get
                {
                    return true;
                }
            }

            public bool IsContextedItemNotSelected
            {
                get
                {
                    return !SelectedBinItems.Contains(ContextedItem);
                }
            }

            public bool IsContextedItemSelected
            {
                get
                {
                    return SelectedBinItems.Contains(ContextedItem);
                }
            }

            public bool IsContextedItemNotInArchive
            {
                get
                {
                    if (ContextedItem != null && Engine.Env.ArchiveBinPath != null)
                        return ContextedItem.Path != Engine.Env.ArchiveBinPath;
                    else
                        return false;
                }
            }

            private List<IBinItem> _selectedBinItems;
            public List<IBinItem> SelectedBinItems
            {
                get
                {
                    return _selectedBinItems;
                }
                set
                {
                    _selectedBinItems = value;
                    UpdateSelection();
                }
            }

            public void ToggleSelectItem(IBinItem item, bool add = false)
            {
                if (SelectedBinItems.Contains(item))
                    SelectedBinItems.Remove(item);
                else
                {
                    if (!add)
                        SelectedBinItems.Clear();

                    SelectedBinItems.Add(item);
                }
                UpdateSelection();
            }

            public void SelectItem(IBinItem item)
            {
                if (!SelectedBinItems.Contains(item))
                    SelectedBinItems.Add(item);

                UpdateSelection();
            }

            public void ClearSelection()
            {
                SelectedBinItems.Clear();
                UpdateSelection();
            }

            public bool HasSelected
            {
                get { return SelectedBinItems.Count > 0; }
            }

            public bool HasOneSelected
            {
                get { return SelectedBinItems.Count == 1; }
            }

            public bool HasOnlyBinSelected
            {
                get
                {
                    foreach (IBinItem item in SelectedBinItems)
                    {
                        if (!(item is BinModel))
                            return false;
                    }
                    return true;
                }
            }

            public bool HasOnlyOneBinSelected
            {
                get
                {
                    return SelectedBinItems.Count == 1 && SelectedBinItems[0] is BinModel;
                }
            }

            public bool HasOnlyOneBookmarkSelected
            {
                get
                {
                    return SelectedBinItems.Count == 1 && SelectedBinItems[0] is BookmarkModel;
                }
            }

            public bool HasOnlyBookmarkSelected
            {
                get
                {
                    foreach (IBinItem item in SelectedBinItems)
                    {
                        if (!(item is BookmarkModel))
                            return false;
                    }
                    return true;
                }
            }

            public BookmarksTreeViewControlModel()
            {
                _selectedBinItems = new List<IBinItem>();
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public static readonly DependencyProperty BinProperty =
            DependencyProperty.Register(
            "Bin", typeof(BinModel),
            typeof(BookmarksTreeViewControl)
        );

        public BinModel Bin
        {
            get { return (BinModel)GetValue(BinProperty); }
            set { SetValue(BinProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
            "Filter", typeof(string),
            typeof(BookmarksTreeViewControl)
        );

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public BookmarksTreeViewControl()
        {
            InitializeComponent();
            Model = new BookmarksTreeViewControlModel();
        }


        enum EEditTextboxAction
        {
            Rename, //Unused
            ChangePath,
        }

        private EEditTextboxAction _editTextboxAction;
        public BookmarksTreeViewControlModel Model { get; set; }

        private void OpenLink(string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result) Process.Start(new ProcessStartInfo(uriResult.AbsoluteUri));
        }

        private void ShowEditBin(object sender, string fill)
        {
            MenuItem mnu = sender as MenuItem;
            StackPanel spParent = null;
            if (mnu != null)
            {
                spParent = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            }
            TextBlock txtBlock = (TextBlock)spParent.FindName("lbl_name");
            TextBox tbRename = (TextBox)spParent.FindName("tb_rename");
            tbRename.Text = fill;
            Env.ExplicitFocus(tbRename);
            tbRename.Visibility = Visibility.Visible;
            txtBlock.Visibility = Visibility.Collapsed;
        }

        private string EndEdit(object sender, KeyEventArgs e)
        {
            TextBox txtBox = ((TextBox)sender);
            if (e.Key == Key.Enter)
            {
                if (((TextBox)e.Source).DataContext is BinModel)
                {
                    StackPanel spParent = (StackPanel)VisualTreeHelper.GetParent(txtBox);
                    TextBlock lblRename = (TextBlock)spParent.FindName("lbl_name");

                    lblRename.Visibility = Visibility.Visible;
                    txtBox.Visibility = Visibility.Collapsed;
                    return txtBox.Text;
                }
            }
            if (e.Key == Key.Escape)
            {
                StackPanel spParent = (StackPanel)VisualTreeHelper.GetParent(txtBox);
                TextBlock lblRename = (TextBlock)spParent.FindName("lbl_name");

                lblRename.Visibility = Visibility.Visible;
                txtBox.Visibility = Visibility.Collapsed;
                return txtBox.Text;
            }

            return "";
        }

        private void Tb_rename_KeyDown(object sender, KeyEventArgs e)
        {
            string txt = EndEdit(sender, e);
            if (txt != "")
            {
                BinModel model = ((BinModel)((TextBox)e.Source).DataContext);

                if (_editTextboxAction == EEditTextboxAction.ChangePath) Engine.Env.BinController.ChangePath(model, txt);
            }
        }

        private void Mi_rename_Click(object sender, RoutedEventArgs e)
        {
            ShowEditBin(sender, ((IBinItem)((MenuItem)sender).DataContext).Path);
            _editTextboxAction = EEditTextboxAction.Rename;
        }

        private async void Mi_delete_ClickAsync(object sender, RoutedEventArgs e)
        {
            object dataContextedElement = ((MenuItem)sender).DataContext;
            if (dataContextedElement is BinModel)
            {
                int result = await View.MessageBox.FireAsync(
                    TranslationSource.Instance["DeleteBin"],
                    TranslationSource.Instance["DeleteBinQuestion"],
                    new List<string>()
                    {
                        TranslationSource.Instance["Yes"],
                        TranslationSource.Instance["No"]
                    }
                );
                if (result == 0)
                {
                    Engine.Env.BinController.DeleteBin((BinModel)dataContextedElement);
                }
            }
            else if (dataContextedElement is BookmarkModel)
            {
                int result = await View.MessageBox.FireAsync(
                    TranslationSource.Instance["DeleteBookmark"],
                    TranslationSource.Instance["DeleteBookmarkQuestion"],
                new List<string>()
                {
                    TranslationSource.Instance["Yes"],
                    TranslationSource.Instance["No"]
                });
                if (result == 0)
                {
                    Engine.Env.BinController.DeleteBookmark(((BookmarkModel)dataContextedElement).ID);
                }
            }
        }

        private void Mi_changePath_Click(object sender, RoutedEventArgs e)
        {
            ShowEditBin(sender, ((IBinItem)((MenuItem)sender).DataContext).Path);
            _editTextboxAction = EEditTextboxAction.ChangePath;
        }

        private void ExecuteBookmarkAction(BookmarkModel bookmark)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    Model.ToggleSelectItem(bookmark, true);
                }
                else
                {
                    Model.ToggleSelectItem(bookmark);
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if (bookmark.Type == "text")
                {
                    ClipboardManager.CopyToClipboard(ClipboardManager.GetClipboardText() + " " + bookmark.Content);
                }
                else if (bookmark.Type == "link" || 
                         bookmark.Type == "image" ||
                         bookmark.Type == "dir" ||
                         bookmark.Type == "file")
                {
                    ClipboardManager.CopyToClipboard(bookmark.Content);
                }
            }
            else
            {
                if (bookmark.Type == "text")
                {
                    ClipboardManager.CopyToClipboard(bookmark.Content);
                }
                else if (bookmark.Type == "link" || 
                         bookmark.Type == "image")
                {
                    OpenLink(bookmark.Content);
                }
                else if(bookmark.Type == "dir" || 
                        bookmark.Type == "file")
                {
                    Process.Start(bookmark.Content);
                }
            }
        }

        private void Mi_ExecuteBookmarkAction_Click(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;
            ClipboardManager.CopyToClipboard(bookmark.Content);
        }

        private void Mi_BookmarkDetails_Click(object sender, RoutedEventArgs e)
        {
            BookmarkDetailsWnd wnd = new BookmarkDetailsWnd((BookmarkModel)((MenuItem)sender).DataContext);
            wnd.Show();
        }

        private async void Mi_BookmarkDelete_ClickAsync(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;

            int result = await View.MessageBox.FireAsync(
                TranslationSource.Instance["DeleteBookmark"],
                TranslationSource.Instance["DeleteBookmarkQuestion"],
                new List<string>()
                {
                    TranslationSource.Instance["Yes"],
                    TranslationSource.Instance["No"]
                });

            if (result == 0)
            {
                Engine.Env.FirebaseController.DeleteBookmark(bookmark);
            }
        }

        private void Mi_copyPath_Click(object sender, RoutedEventArgs e)
        {
            ClipboardManager.CopyToClipboard(((IBinItem)((MenuItem)sender).DataContext).Path);
        }

        private void Mi_CopyAdditive_Click(object sender, RoutedEventArgs e)
        {
            if (Model.HasOneSelected)
            {
                BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;
                ClipboardManager.CopyToClipboard(ClipboardManager.GetClipboardText() + " " + bookmark.Content);
            }
            else
            {
                string toCopy = " ";
                foreach (IBinItem item in Model.SelectedBinItems)
                {
                    if (item is BookmarkModel) toCopy += " " + ((BookmarkModel)item).Content;
                    else if (item is BinModel) toCopy += " " + ((BinModel)item).Name;
                }
                ClipboardManager.CopyToClipboard(toCopy);
            }
        }

        private void MoveSelected(string path)
        {
            if (Model.HasSelected)
            {
                foreach (IBinItem item in Model.SelectedBinItems)
                {
                    Engine.Env.BinController.ChangePath(item, path);
                }
            }
        }

        public delegate void ExplicitFilterHandler(string filter);
        public event ExplicitFilterHandler ExplicitFilter;

        private void Rt_bin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((StackPanel)sender).DataContext;
            if (Keyboard.Modifiers == ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                Model.ToggleSelectItem(sourceBin);
            }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                Model.SelectItem(sourceBin);
            }
            else
            {
                string sourcePath = sourceBin.Path;

                ExplicitFilter?.Invoke("-p " + sourcePath);
            }
        }

        private void QueryNet(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string param = (string)menuItem.CommandParameter;
            IBinItem binItem = (IBinItem)((MenuItem)sender).DataContext;

            if (binItem is BookmarkModel)
            {
                BookmarkModel bookmark = (BookmarkModel)binItem;
                string content = "";
                if (bookmark.Caption != "")
                    content = bookmark.Caption;
                else
                    content = bookmark.Content;

                OpenLink(param + content);
            }
            else
            {
                OpenLink(param + binItem.Path.Replace(Engine.Env.PathSeparator, ' '));
            }
        }

        private void Mi_Deselect_Click(object sender, RoutedEventArgs e)
        {
            Model.ClearSelection();
        }

        private void RootControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Model.ClearSelection();
            }
        }

        private void Mi_selectItems_Click(object sender, RoutedEventArgs e)
        {
            Model.ClearSelection();
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;

            if (sourceBin != null)
            {
                Model.SelectItem(sourceBin);
            }
        }

        private void Mi_selectItemsChildren_Click(object sender, RoutedEventArgs e)
        {
            Model.ClearSelection();
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;

            if (sourceBin != null)
            {
                if (sourceBin is BookmarkModel)
                {
                    sourceBin = Engine.Env.BinController.GetBin(sourceBin.Path);
                }

                if (sourceBin is BinModel)
                {
                    var pathChildren = Engine.Env.BinController.GetItemsAndChildren((BinModel)sourceBin);
                    foreach (var item in pathChildren)
                    {
                        Model.SelectItem(item.Value);
                    }
                }
            }
        }

        private void Btn_BookmarkAction_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                FrameworkElement btn = (FrameworkElement)sender;

                IBinItem binItem = (IBinItem)btn.DataContext;

                BinItemPreviewWnd.Instance.SetBinItem(binItem);
                BinItemPreviewWnd.Instance.Show();
                //Window.GetWindow(this).Activate();
            }
        }

        private void Btn_BookmarkAction_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                FrameworkElement btn = (FrameworkElement)sender;
                if (btn.DataContext is IBinItem)
                {
                    IBinItem bookmark = (IBinItem)btn.DataContext;
                    if (BinItemPreviewWnd.Instance.IsVisible)
                    {
                        BinItemPreviewWnd.Instance.Hide();
                    }
                }
            }
        }

        private static double Clamp(double x, double a, double b)
        {
            return Math.Min(Math.Max(x, a), b);
        }

        private void Btn_BookmarkAction_MouseMove(object sender, MouseEventArgs e)
        {
            if (BinItemPreviewWnd.Instance.IsVisible)
            {
                Point mousePos = GetMousePosition();
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)mousePos.X, (int)mousePos.Y));

                BinItemPreviewWnd.Instance.Left  = Clamp(mousePos.X + 20, screen.Bounds.Left, screen.Bounds.Left + screen.Bounds.Width - BinItemPreviewWnd.Instance.Width);
                BinItemPreviewWnd.Instance.Top = Clamp(mousePos.Y + 20, screen.Bounds.Top, screen.Bounds.Top + screen.Bounds.Height - BinItemPreviewWnd.Instance.Height);
            }
        }

        private void Mi_addBookmarkToBin_Click(object sender, RoutedEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;
            ((CreateBookmarkWnd)Env.MainWindow.ChildWindows[typeof(CreateBookmarkWnd)]).Fire(sourceBin.Path);
        }

        private void Mi_Select_Click(object sender, RoutedEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;
            Model.ToggleSelectItem(sourceBin);
        }

        private void Mi_SelectAdd_Click(object sender, RoutedEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;
            Model.SelectItem(sourceBin);
        }

        private void Mi_MoveSelection_Click(object sender, RoutedEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;
            string sourcePath = sourceBin.Path;
            MoveSelected(sourcePath);
        }
        public void ExecAnim()
        {
            if (Engine.Env.UserData.CustomModel.AnimationsLevel == 2) mah_contentControl.Reload();
        }

        private void Mi_Archive_Click(object sender, RoutedEventArgs e)
        {
            foreach (IBinItem sourceBin in Model.SelectedBinItems)
            {
                Engine.Env.BinController.ChangePath(sourceBin, Engine.Env.ArchiveBinPath);
            }
        }

        private void Btn_BookmarkAction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            object context = ((FrameworkElement)sender).DataContext;

            if (context is IBinItem)
            {
                IBinItem bookmark = (IBinItem)context;
                if (Mouse.RightButton == MouseButtonState.Pressed)
                {
                    if (Model.HasSelected && bookmark is BinModel)
                    {

                    }
                    else if (Model.SelectedBinItems.Count <= 1)
                    {
                        Model.ClearSelection();
                        Model.SelectItem(bookmark);
                    }
                }
            }
        }

        private void Btn_BookmarkAction_Click(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((Button)sender).DataContext;
            ExecuteBookmarkAction(bookmark);
        }

        private void Mah_contentControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(((FrameworkElement)e.OriginalSource).DataContext is IBinItem))
            {
                Model.ClearSelection();
            }
        }

        private void BinItemContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Model.ContextedItem = (IBinItem)((FrameworkElement)sender).DataContext;
        }

        private void Mi_SelectAddAll_Click(object sender, RoutedEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((MenuItem)sender).DataContext;

            if (sourceBin != null)
            {
                if (sourceBin is BookmarkModel)
                {
                    sourceBin = Engine.Env.BinController.GetBin(sourceBin.Path);
                }

                if (sourceBin is BinModel)
                {
                    var pathChildren = Engine.Env.BinController.GetItemsAndChildren((BinModel)sourceBin);
                    foreach (var item in pathChildren)
                    {
                        Model.SelectItem(item.Value);
                    }
                }
            }
        }
    }
}

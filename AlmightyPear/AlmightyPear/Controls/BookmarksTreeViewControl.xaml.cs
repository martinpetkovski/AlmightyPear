using AlmightyPear.Controller;
using AlmightyPear.Model;
using AlmightyPear.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AlmightyPear.Controls
{
    

    public partial class BookmarksTreeViewControl : UserControl
    {
        public class BookmarksTreeViewControlModel : INotifyPropertyChanged
        {
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
                    OnPropertyChanged();
                }
            }

            public void SelectItem(IBinItem item, bool add = false)
            {
                if (SelectedBinItems.Contains(item))
                    SelectedBinItems.Remove(item);
                else
                {
                    if (!add)
                        SelectedBinItems.Clear();

                    SelectedBinItems.Add(item);
                }
                OnPropertyChanged("SelectedBinItems");
            }

            public void ClearSelection()
            {
                SelectedBinItems.Clear();
                OnPropertyChanged("SelectedBinItems");
            }

            public bool HasSelected()
            {
                return SelectedBinItems.Count > 0;
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

        public static readonly DependencyProperty FilterTextBoxProperty =
            DependencyProperty.Register(
            "FilterTextBox", typeof(TextBox),
            typeof(BookmarksTreeViewControl)
        );

        public TextBox FilterTextBox
        {
            get { return (TextBox)GetValue(FilterTextBoxProperty); }
            set { SetValue(FilterTextBoxProperty, value); }
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
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
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

                if (_editTextboxAction == EEditTextboxAction.ChangePath) Env.BinController.ChangePath(model, txt);
            }
        }

        private void Mi_rename_Click(object sender, RoutedEventArgs e)
        {
            ShowEditBin(sender, ((IBinItem)((MenuItem)sender).DataContext).Path);
            _editTextboxAction = EEditTextboxAction.Rename;
        }

        private void Mi_delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("You are about to delete this bin along with all containing bookmarks. \nThis action is irreversible. \nAre you sure you want to continue?", "Delete Bin", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Env.BinController.DeleteBin((BinModel)((MenuItem)sender).DataContext);
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
                    Model.SelectItem(bookmark, true);
                }
                else
                {
                    Model.SelectItem(bookmark);
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if (bookmark.Type == "text")
                {
                    Env.CopyToClipboard(Env.GetClipboardText() + " " + bookmark.Content);
                }
                else if (bookmark.Type == "link")
                {
                    Env.CopyToClipboard(bookmark.Content);
                }
            }
            else
            {
                if (bookmark.Type == "text")
                {
                    Env.CopyToClipboard(bookmark.Content);
                }
                else if (bookmark.Type == "link")
                {
                    OpenLink(bookmark.Content);
                }
            }
        }

        private void Btn_BookmarkAction_Click(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((Button)sender).DataContext;
            ExecuteBookmarkAction(bookmark);
        }

        private void Mi_ExecuteBookmarkAction_Click(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;
            Env.CopyToClipboard(bookmark.Content);
        }

        private void Mi_BookmarkDetails_Click(object sender, RoutedEventArgs e)
        {
            BookmarkDetailsWnd wnd = new BookmarkDetailsWnd((BookmarkModel)((MenuItem)sender).DataContext);
            wnd.Show();
        }

        private void Mi_BookmarkDelete_Click(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;

            MessageBoxResult result = MessageBox.Show("You are about to delete this bookmark. \nThis action is irreversible. \nAre you sure you want to continue?", "Delete Bookmark", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Env.BinController.DeleteBookmark(bookmark);
            }
        }

        private void Mi_copyPath_Click(object sender, RoutedEventArgs e)
        {
            Env.CopyToClipboard(((BinModel)((MenuItem)sender).DataContext).Path);
        }

        private void Mi_CopyAdditive_Click(object sender, RoutedEventArgs e)
        {
            BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;
            Env.CopyToClipboard(Env.GetClipboardText() + " " + bookmark.Content);
        }

        private void Rt_bin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IBinItem sourceBin = (IBinItem)((StackPanel)sender).DataContext;
            string sourcePath = sourceBin.Path;

            if (Model.HasSelected())
            {
                foreach (IBinItem item in Model.SelectedBinItems)
                {
                    Env.BinController.ChangePath(item, sourcePath);
                }
            }
            else
            {
                if(FilterTextBox != null)
                    FilterTextBox.Text = "-p " + sourcePath;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 10);
            e.Handled = true;
        }

        private void QueryNet(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string param = (string)menuItem.CommandParameter;
            BookmarkModel bookmark = (BookmarkModel)((MenuItem)sender).DataContext;

            string content = "";
            if (bookmark.Caption != "")
                content = bookmark.Caption;
            else
                content = bookmark.Content;

            OpenLink(param + content);
        }

        private void Mi_Deselect_Click(object sender, RoutedEventArgs e)
        {
            Model.ClearSelection();
        }

        private void RootControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Model.ClearSelection();
            }
        }

        public void ExecTop()
        {
            
        }
    }
}

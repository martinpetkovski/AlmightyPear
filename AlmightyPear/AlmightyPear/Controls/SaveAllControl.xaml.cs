using AlmightyPear.Controller;
using AlmightyPear.View;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MessageBox = AlmightyPear.View.MessageBox;

namespace AlmightyPear.Controls
{
    /// <summary>
    /// Interaction logic for SaveAllControl.xaml
    /// </summary>
    public partial class SaveAllControl : UserControl
    {
        public static readonly DependencyProperty CreateBookmarkControlProperty =
           DependencyProperty.Register(
           "CreateBookmarkControl", typeof(BookmarkCreateControl),
           typeof(SaveAllControl)
       );

        public BookmarkCreateControl CreateBookmarkControl
        {
            get { return (BookmarkCreateControl)GetValue(CreateBookmarkControlProperty); }
            set { SetValue(CreateBookmarkControlProperty, value); }
        }

        public SaveAllControl()
        {
            InitializeComponent();
        }

        private async void Btn_SaveAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (Env.BinController.HasEditedBookmarks() || Env.BinData.HasCreatedBookmarks)
            {
                string pluralToken = Env.BinController.EditedBookmarks.Count == 1 ? "" : "s";
                string token = Env.BinController.EditedBookmarks.Count + " edited bookmark" + pluralToken + " and the newly created bookmark";
                if(!Env.BinController.HasEditedBookmarks() && Env.BinData.HasCreatedBookmarks)
                {
                    token = "the newly created bookmark";
                }
                else if(Env.BinController.HasEditedBookmarks() && !Env.BinData.HasCreatedBookmarks)
                {
                    token = Env.BinController.EditedBookmarks.Count + " edited bookmark" + pluralToken;
                }

                int result = await MessageBox.FireAsync("Save All",
                    "You are about to save " + token + "." +
                    "\nAre you sure you want to proceed?",
                    new List<string>() { "Yes", "No" });

                if (result == 0)
                {
                    Env.ClearClipboard();
                    ProgressBarWnd.FireAsync();
                    await Env.BinController.SaveEditedBookmarksAsync(ProgressBarWnd.Instance, ProgressBarWnd.UpdateProgress);

                    await CreateBookmarkControl.CreateAsync();
                }
            }
            else
            {
                await MessageBox.FireAsync("Save All",
                   "Nothing to save.",
                   new List<string>() { "Ok" });
            }
        }
    }
}

using AlmightyPear.Controller;
using System.Windows;
using System.Windows.Controls;

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

        public void UpdateProgress(double progress, string status)
        {
            pb_SaveProgress.Value = progress;
        }

        private async void Btn_SaveAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            Env.ClearClipboard();
            Env.BinController.SaveEditedBookmarks(UpdateProgress);
            pb_SaveProgress.Value = 0;

            await CreateBookmarkControl.CreateAsync();
        }
    }
}

using AlmightyPear.Controller;
using AlmightyPear.View;
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

        private async void Btn_SaveAll_ClickAsync(object sender, RoutedEventArgs e)
        {
            Env.ClearClipboard();
            ProgressBarWnd.FireAsync();
            await Env.BinController.SaveEditedBookmarksAsync(ProgressBarWnd.Instance, ProgressBarWnd.UpdateProgress);

            await CreateBookmarkControl.CreateAsync();
        }
    }
}

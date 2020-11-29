using AlmightyPear.Controller;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlmightyPear.Controls
{
    /// <summary>
    /// Interaction logic for BookmarksViewControl.xaml
    /// </summary>
    public partial class BookmarksViewControl : UserControl
    {
        public class BookmarksViewModel : INotifyPropertyChanged
        {
            private string _delayedText;
            public string DelayedText
            {
                get
                {
                    return _delayedText;
                }

                set
                {
                    _delayedText = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }


        [Browsable(true)]
        [Category("Action")]
        [Description("Invoked when user clicks button")]
        public event EventHandler FilterEvent;

        public static readonly DependencyProperty MaxContentHeightProperty =
            DependencyProperty.Register(
            "MaxContentHeight", typeof(int),
            typeof(BookmarksTreeViewControl),
            new FrameworkPropertyMetadata(default(int))
        );

        public int MaxContentHeight
        {
            get { return (int)GetValue(MaxContentHeightProperty); }
            set { SetValue(MaxContentHeightProperty, value); }
        }

        public static readonly DependencyProperty ContentTextSizeProperty =
            DependencyProperty.Register(
            "ContentTextSize", typeof(int),
            typeof(BookmarksViewControl),
            new PropertyMetadata(10)
        );

        public int ContentTextSize
        {
            get { return (int)GetValue(ContentTextSizeProperty); }
            set { SetValue(ContentTextSizeProperty, value); }
        }

        public BookmarksViewControl()
        {
            InitializeComponent();
            Model = new BookmarksViewModel();
        }

        public BookmarksViewModel Model { get; set; }

        private CancellationTokenSource _cts;
        private void Tb_filter_KeyDown(object sender, KeyEventArgs e)
        {
            FilterEvent?.Invoke(sender, e);
            if (e.Key == Key.Escape)
            {
                tb_filter.Clear();
            }
            if (e.Key == Key.Tab)
            {
                tb_filter.CaretIndex = tb_filter.Text.Length;
                e.Handled = true;
            }
        }

        private static bool isExplicitRefresh = false;
        private void FinishedTyping()
        {
            if (!isExplicitRefresh)
            {
                Model.DelayedText = tb_filter.Text;
                ctrl_BookmarksTreeView.ExecAnim();
            }

            isExplicitRefresh = false;
        }

        private void Btn_clearFilter_Click(object sender, RoutedEventArgs e)
        {
            tb_filter.Clear();
        }

        private async void Tb_filter_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var cancellationToken = _cts.Token;
                await Task.Delay(200, cancellationToken);
                FinishedTyping();
            }
            catch (TaskCanceledException ex)
            {
                // Ignore the exception
            }
        }

        private void Ctrl_BookmarksTreeView_ExplicitFilter(string filter)
        {
            tb_filter.Clear();
            tb_filter.Text = filter;
            isExplicitRefresh = true;
            Model.DelayedText = filter;
            ctrl_BookmarksTreeView.ExecAnim();

            Env.ExplicitFocus(tb_filter);
        }
    }
}

using AlmightyPear.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void FinishedTyping()
        {
            
            Model.DelayedText = tb_filter.Text;
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
    }
}

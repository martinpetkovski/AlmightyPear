using AlmightyPear.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        public BookmarksViewControl()
        {
            InitializeComponent();
        }

        private void Tb_filter_KeyDown(object sender, KeyEventArgs e)
        {
            FilterEvent?.Invoke(sender, e);
        }
    }
}

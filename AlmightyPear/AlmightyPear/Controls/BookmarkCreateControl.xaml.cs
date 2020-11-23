using AlmightyPear.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for BookmarkCreateControl.xaml
    /// </summary>
    public partial class BookmarkCreateControl : UserControl
    {
        [Browsable(true)]
        [Category("Action")]
        [Description("Invoked when user clicks button")]
        public event EventHandler InputEvent;

        public BookmarkCreateControl()
        {
            InitializeComponent();
        }

        public async Task CreateAsync()
        {
            if (tb_category.Text.Length == 0 || txt_selection.Text.Length == 0)
                return;

            await Env.FirebaseController.CreateBookmarkAsync(tb_category.Text, txt_selection.Text);
            Env.ClearClipboard();
        }

        public void Cancel()
        {
            txt_selection.Text = "";
            tb_category.Clear();
            Env.ClearClipboard();
        }

        public void Initialize()
        {
            string selectedText = Env.GetClipboardText();

            txt_selection.Text = selectedText;

        }

        private void Tb_category_KeyDownAsync(object sender, KeyEventArgs e)
        {
            InputEvent?.Invoke(sender, e);
        }
    }
}

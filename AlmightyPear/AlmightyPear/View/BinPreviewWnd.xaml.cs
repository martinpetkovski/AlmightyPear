using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AlmightyPear.View
{
    /// <summary>
    /// Interaction logic for ImagePreviewWnd.xaml
    /// </summary>
    public partial class BinPreviewWnd : Window
    {
        public class BinPreviewWndModel : INotifyPropertyChanged
        {
            private string _imageUrl;
            public string ImageUrl
            {
                get
                {
                    return _imageUrl;
                }
                set
                {
                    _imageUrl = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public BinPreviewWndModel Model { get; set; }

        public BinPreviewWnd(string imageUrl)
        {
            Model = new BinPreviewWndModel();
            Model.ImageUrl = imageUrl;
            InitializeComponent();
        }
    }
}

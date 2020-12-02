using AlmightyPear.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AlmightyPear.View
{


    public partial class BinItemPreviewWnd : Window
    {
        private static BinItemPreviewWnd _instance;
        public static BinItemPreviewWnd Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BinItemPreviewWnd();

                return _instance;
            }
        }

        public class BinItemPreviewWndModel : INotifyPropertyChanged
        {
            private IBinItem _binItem;
            public IBinItem BinItem
            {
                get
                {
                    return _binItem;
                }
                set
                {
                    _binItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged("BinItemType");
                }
            }

            public string BinItemType
            {
                get
                {
                    if (BinItem is BinModel) return "bin";
                    else if (BinItem is BookmarkModel) return ((BookmarkModel)BinItem).Type;
                    else return "bin";
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public BinItemPreviewWndModel Model { get; set; }

        public BinItemPreviewWnd()
        {
            InitializeComponent();
        }

        public void SetBinItem(IBinItem binItem)
        {
            if(Model == null)
                Model = new BinItemPreviewWndModel();

            Model.BinItem = binItem;
        }
    }
}

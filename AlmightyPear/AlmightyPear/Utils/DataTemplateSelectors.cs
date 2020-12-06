using Checkmeg.WPF.Model;
using Checkmeg.WPF.View;
using System.Windows;
using System.Windows.Controls;

namespace Checkmeg.WPF.Utils
{
    public class BinItemPreviewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BinDataTemplate { get; set; }
        public DataTemplate TextDataTemplate { get; set; }
        public DataTemplate LinkDataTemplate { get; set; }
        public DataTemplate ImageDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (BinItemPreviewWnd.Instance.Model != null &&
                BinItemPreviewWnd.Instance.Model.BinItemType == null) return TextDataTemplate;

            string type = "";
            if (BinItemPreviewWnd.Instance.Model != null)
                type = BinItemPreviewWnd.Instance.Model.BinItemType;

            if (type == "bin")
                return BinDataTemplate;
            else if (type == "text")
                return TextDataTemplate;
            else if (type == "link")
                return LinkDataTemplate;
            else if (type == "image")
                return ImageDataTemplate;
            else return TextDataTemplate;

        }
    }
}

using AlmightyPear.Controller;
using AlmightyPear.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AlmightyPear.Converters
{
    class BookmarkCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = "";
            if (value is BookmarkModel)
            {
                BookmarkModel bookmark = (BookmarkModel)value;
                if (bookmark.Type == "link")
                {
                    retVal = bookmark.Caption;
                }
                else
                {
                    retVal = bookmark.Content;
                }
            }

            retVal = retVal.TrimStart(' ', '\n', '\t');
            retVal = retVal.TrimEnd(' ', '\n', '\t');

            if (retVal.Length > 40)
            {
                retVal = retVal.Substring(0, 40);

                retVal += "...";
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class UnixTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long)
            {
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long seconds = (long)value;
                DateTime result = epoch.AddSeconds(seconds);
                return result.ToShortDateString() + " at " + result.ToShortTimeString();
            }

            return new DateTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BookmarkSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is List<IBinItem> &&
               values[1] is BookmarkModel &&
               parameter is string)
            {
                List<IBinItem> selectedItems = (List<IBinItem>)values[0];
                BookmarkModel bookmark = (BookmarkModel)values[1];
                string type = (string)parameter;

                if (type == "Brush")
                {
                    return selectedItems.Contains(bookmark) ?
                        new SolidColorBrush(Colors.ForestGreen) :
                        new SolidColorBrush(Colors.Transparent);
                }

            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BinDepthListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string)
            {
                string path = (string)value;
                int depth = path.Split(Env.PathSeparator).Length;
                if (Env.BinData.BinsByDepth.ContainsKey(depth))
                {
                    return Env.BinData.BinsByDepth[depth];
                }
                else
                    return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
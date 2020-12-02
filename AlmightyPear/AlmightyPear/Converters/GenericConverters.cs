using AlmightyPear.Controller;
using AlmightyPear.Model;
using AlmightyPear.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
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

    class BinItemSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is List<IBinItem> &&
               values[1] is IBinItem &&
               parameter is string)
            {
                List<IBinItem> selectedItems = (List<IBinItem>)values[0];
                IBinItem bookmark = (IBinItem)values[1];
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
            if (value is string)
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

    class IsLoggedInToStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLoggedIn = false;
            if (value is bool)
                isLoggedIn = (bool)value;
            else
                isLoggedIn = Env.UserData.IsLoggedIn;

            bool invert = false;
            if (parameter != null && parameter is string)
            {
                bool.TryParse((string)parameter, out invert);
            }

            if (targetType == typeof(Visibility))
            {
                bool isVisible = isLoggedIn;
                isVisible = invert ? !isVisible : isVisible;
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            if (targetType == typeof(bool))
            {
                return invert ? !isLoggedIn : isLoggedIn;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                bool invert = false;
                if (parameter != null && parameter is string)
                {
                    bool.TryParse((string)parameter, out invert);
                }

                bool b = (bool)value;

                if (invert)
                    b = !b;

                return b ? Visibility.Visible : Visibility.Collapsed;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BoolToMenuItemVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is bool && values[1] is bool)
            {
                bool add = true;
                if(values.Length >=3 && values[2] is bool)
                {
                    add = (bool)values[2];
                }
                return ((bool)values[0] && (bool)values[1] && add) ? Visibility.Visible : Visibility.Collapsed;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class AnimationValueToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double)
            {
                double animValue = (double)value;
                if (animValue == 0)
                    return "off";
                else if (animValue == 1)
                    return "non-distracting";
                else if (animValue == 2)
                    return "beautiful";
            }
            return "off the scale";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class AnimationLevelToBoolenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is int && parameter is string)
            {
                int level = (int)value;
                int expectedLevel = int.Parse((string)parameter);
                return level >= expectedLevel;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ThemeNameToThemeReferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string themeName = (string)value;

                if(Env.UserData.Themes.ContainsKey(themeName))
                    return Env.UserData.Themes[themeName];
            }

            if (Env.UserData.ThemesList.Count > 0)
                return Env.UserData.ThemesList[0];
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is ThemeManager.Theme)
            {
                return ((ThemeManager.Theme)value).Name;
            }

            return "BasicBlack";
        }
    }

    class YoutubeToAddressAutoplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string)
            {
                string url = ((string)value).Trim(' ');
                string youtubeAddressRegex = "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))([\\w-]{10,12})(?:$|\\&|\\?\\#).*";
                if (Regex.IsMatch(url, youtubeAddressRegex))
                {
                    Match match = Regex.Match(url, youtubeAddressRegex);
                    return "https://www.youtube.com/embed/" + match.Groups[1] + "/?autoplay=1";
                }
                else
                    return value;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
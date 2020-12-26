using Checkmeg.WPF.Controller;
using Checkmeg.WPF.Model;
using Checkmeg.WPF.Utils;
using Engine;
using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Env = Checkmeg.WPF.Controller.Env;

namespace Checkmeg.WPF.Converters
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
                return result.ToString(TranslationSource.Instance.CurrentCulture.DateTimeFormat.LongDatePattern) + 
                    " @ " + 
                    result.ToString(TranslationSource.Instance.CurrentCulture.DateTimeFormat.ShortTimePattern);
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
                int depth = path.Split(Engine.Env.PathSeparator).Length;
                if (Engine.Env.BinData.BinsByDepth.ContainsKey(depth))
                {
                    return Engine.Env.BinData.BinsByDepth[depth];
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
                isLoggedIn = Engine.Env.UserData.IsLoggedIn;

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
            if (value is bool)
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
                if (values.Length >= 3 && values[2] is bool)
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

    class AnimationValueToNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double)
            {
                double animValue = (double)values[0];
                if (animValue == 0)
                    return TranslationSource.Instance["Off"];
                else if (animValue == 1)
                    return TranslationSource.Instance["NonDistracting"];
                else if (animValue == 2)
                    return TranslationSource.Instance["Beautiful"];
            }
            return "off the scale";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class AnimationLevelToBoolenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && parameter is string)
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

                if (Engine.Env.UserData.Themes.ContainsKey(themeName))
                    return Engine.Env.UserData.Themes[themeName];
            }

            if (Engine.Env.UserData.ThemesList.Count > 0)
                return Engine.Env.UserData.ThemesList[0];
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThemeModel)
            {
                return ((ThemeModel)value).Name;
            }

            return "BasicBlack";
        }
    }

    class YoutubeToAddressAutoplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string url = ((string)value).Trim(' ');
                string youtubeAddressRegex = "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))([\\w-]{10,12})(?:$|\\&|\\?\\#).*";
                if (Regex.IsMatch(url, youtubeAddressRegex))
                {
                    Match match = Regex.Match(url, youtubeAddressRegex);
                    return "https://img.youtube.com/vi/" + match.Groups[1] + "/maxresdefault.jpg";
                }
                else
                {
                    return Env.GetHeaderImageFromUrl(url);
                }

            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BookmarkTypeToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string type = (string)value;
                if (type == "link")
                    return FontAwesomeIcon.Link;
                else if (type == "text")
                    return FontAwesomeIcon.FileText;
                else if (type == "image")
                    return FontAwesomeIcon.Image;
                else if (type == "dir")
                    return FontAwesomeIcon.Folder;
                else if (type == "file")
                    return FontAwesomeIcon.File;
            }

            return FontAwesomeIcon.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class LanguageShorthandToFullName : IValueConverter
    {
        private static Dictionary<string, string> Langs = new Dictionary<string, string>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string lang = (string)value;
                string nativeName = new CultureInfo(lang).NativeName;
                Langs[nativeName] = lang;
                return nativeName;
            }
            else if(value is CultureInfo)
            {
                string nativeName = ((CultureInfo)value).NativeName;
                if (nativeName.Contains("Северна Македонија"))
                    nativeName = nativeName.Replace("Северна Македонија", "Македонија");

                return nativeName;
            }
            else if(value is List<string>)
            {
                List<string> retVal = new List<string>();
                foreach(string lang in (List<string>)value)
                {
                    string nativeName = new CultureInfo(lang).NativeName;
                    if (nativeName.Contains("Северна Македонија"))
                        nativeName = nativeName.Replace("Северна Македонија", "Македонија");
                    Langs[nativeName] = lang;
                    retVal.Add(nativeName);
                }
                return retVal;
            }
            else
                return "English";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string lang = (string)value;
                return Langs[lang];
            }
            else if (value is List<string>)
            {
                List<string> retVal = new List<string>();
                foreach (string lang in (List<string>)value)
                {
                    retVal.Add(Langs[lang]);
                }
                return retVal;
            }
            else
                return "en-US";
        }
    }

    class BinTypeToName : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values[0] is string)
            {
                string type = (string)values[0];
                    return TranslationSource.Instance[type];
            }
            return "Invalid Type";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
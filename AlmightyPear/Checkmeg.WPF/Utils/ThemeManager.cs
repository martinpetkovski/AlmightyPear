using Checkmeg.WPF.Controller;
using Engine;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Checkmeg.WPF.Utils
{
    public class XAMLThemeController : Engine.IThemeController
    {
        private void SetColor(ResourceDictionary rd, string key, string color)
        {

            if (rd.Contains(key))
            {
                if (key.StartsWith("mg.solid") || key.Contains("Brushes"))
                {
                    rd[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                }
                else if (key.StartsWith("mg.mono") || key.Contains("Colors"))
                {
                    rd[key] = ColorConverter.ConvertFromString(color);
                }
            }
        }

        private void UpdateGradient(ResourceDictionary rd, string key, ThemeModel theme)
        {
            if (rd.Contains(key) && rd[key] is LinearGradientBrush)
            {
                LinearGradientBrush newLgb = new LinearGradientBrush();
                if (key == "GradInset")
                {
                     newLgb = new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString(theme.Mono20),
                        (Color)ColorConverter.ConvertFromString(theme.Mono40), 90);
                }

                if (key == "GradOutset")
                {
                    newLgb = new LinearGradientBrush(
                       (Color)ColorConverter.ConvertFromString(theme.Mono40),
                       (Color)ColorConverter.ConvertFromString(theme.Mono20), 90);
                }

                if (key == "GradInsetSide")
                {
                    newLgb = new LinearGradientBrush(
                       (Color)ColorConverter.ConvertFromString(theme.Mono20),
                       (Color)ColorConverter.ConvertFromString(theme.Mono40), 0);
                }

                if (key == "GradOutsetSide")
                {
                    newLgb = new LinearGradientBrush(
                       (Color)ColorConverter.ConvertFromString(theme.Mono40),
                       (Color)ColorConverter.ConvertFromString(theme.Mono20), 0);
                }

                rd[key] = newLgb;

            }
        }

        public void SetTheme(string name )
        {
            if (Engine.Env.UserData.Themes.ContainsKey(name))
            {
                SetTheme(Engine.Env.UserData.Themes[name]);
            }
        }

        public void SetTheme(ThemeModel theme)
        {
            foreach (ResourceDictionary rd in Application.Current.Resources.MergedDictionaries)
            {
                SetColor(rd, "mg.mono.0", theme.Mono0);
                SetColor(rd, "mg.mono.10", theme.Mono10);
                SetColor(rd, "mg.mono.20", theme.Mono20);
                SetColor(rd, "mg.mono.40", theme.Mono40);
                SetColor(rd, "mg.mono.70", theme.Mono70);
                SetColor(rd, "mg.mono.100", theme.Mono100);

                SetColor(rd, "mg.solid.0", theme.Mono0);
                SetColor(rd, "mg.solid.10", theme.Mono10);
                SetColor(rd, "mg.solid.20", theme.Mono20);
                SetColor(rd, "mg.solid.40", theme.Mono40);
                SetColor(rd, "mg.solid.70", theme.Mono70);
                SetColor(rd, "mg.solid.100", theme.Mono100);

                SetColor(rd, "MahApps.Colors.Highlight", theme.Mono40);
                SetColor(rd, "MahApps.Colors.AccentBase", theme.Mono10);
                SetColor(rd, "MahApps.Colors.Accent", theme.Mono10);
                SetColor(rd, "MahApps.Colors.Accent2", theme.Mono20);
                SetColor(rd, "MahApps.Colors.Accent3", theme.Mono40);
                SetColor(rd, "MahApps.Colors.Accent4", theme.Mono70);

                SetColor(rd, "MahApps.Brushes.WindowTitle", theme.Mono10);
                SetColor(rd, "MahApps.Brushes.AccentBase", theme.Mono10);
                SetColor(rd, "MahApps.Brushes.Accent", theme.Mono10);
                SetColor(rd, "MahApps.Brushes.Accent2", theme.Mono20);
                SetColor(rd, "MahApps.Brushes.Accent3", theme.Mono40);
                SetColor(rd, "MahApps.Brushes.Accent4", theme.Mono70);


                UpdateGradient(rd, "GradInset", theme);
                UpdateGradient(rd, "GradOutset", theme);
                UpdateGradient(rd, "GradInsetSide", theme);
                UpdateGradient(rd, "GradOutsetSide", theme);
            }
        }

        public ThemeModel GetBasicDarkTheme()
        {
            ThemeModel t = new ThemeModel();
            t.Name = "BasicDark";
            t.OwnerUser = "deeeeelay@gmail.com";
            t.Mono0 = "#FF000000";
            t.Mono10 = "#FF090909";
            t.Mono20 = "#FF111111";
            t.Mono40 = "#FF222222";
            t.Mono70 = "#FF333333";
            t.Mono100 = "#FFFFFFFF";

            return t;
        }

        public ThemeModel GetBasicRedTheme()
        {
            ThemeModel t = new ThemeModel();
            t.Name = "BasicRed";
            t.OwnerUser = "deeeeelay@gmail.com";
            t.Mono0 = "#FF000000";
            t.Mono10 = "#FF332222";
            t.Mono20 = "#FF443333";
            t.Mono40 = "#FF775555";
            t.Mono70 = "#FFAA9999";
            t.Mono100 = "#FFFFFFFF";

            return t;
        }

        public ThemeModel GetBasicBlueTheme()
        {
            ThemeModel t = new ThemeModel();
            t.Name = "BasicBlue";
            t.OwnerUser = "deeeeelay@gmail.com";
            t.Mono0 = "#FF000000";
            t.Mono10 = "#FF000011";
            t.Mono20 = "#FF0e102b";
            t.Mono40 = "#FF082845";
            t.Mono70 = "#FFEEEEFF";
            t.Mono100 = "#FFFFFFFF";

            return t;
        }

        public ThemeModel GetBasicLightTheme()
        {
            ThemeModel t = new ThemeModel();
            t.Name = "BasicLight";
            t.OwnerUser = "deeeeelay@gmail.com";
            t.Mono0 = "#FFFFFFFF";
            t.Mono10 = "#FFCFCFCF";
            t.Mono20 = "#FFE0E0E0";
            t.Mono40 = "#FFF6F6F6";
            t.Mono70 = "#FFF5F5F5";
            t.Mono100 = "#FF000000";

            return t;
        }

        public async Task CreateBasicThemesAsync()
        {
            

            await Engine.Env.FirebaseController.CreateOrUpdateTheme(GetBasicDarkTheme());
            await Engine.Env.FirebaseController.CreateOrUpdateTheme(GetBasicLightTheme());
            await Engine.Env.FirebaseController.CreateOrUpdateTheme(GetBasicRedTheme());
            await Engine.Env.FirebaseController.CreateOrUpdateTheme(GetBasicBlueTheme());
        }
    }
}

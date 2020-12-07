using Checkmeg.WPF.Model;
using HtmlAgilityPack;
using MahApps.Metro.Controls;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Engine;
using Checkmeg.WPF.Utils;

namespace Checkmeg.WPF.Controller
{
    class Env
    {
        // global
        public static string CheckmegWSCEXE = "CheckmegWSC.exe";
        public static WindowState StartupState = WindowState.Normal;
        // data
        public static EnvModel EnvModel { get; private set; }
        public static MainWindowModel MainWindowData { get; private set; }
        
        // functional
        public static MainWindow MainWindow { get; private set; }
        public static MetroWindow MetroMainWindow { get { return (MetroWindow)MainWindow; } }
        

        public static void Initialize(MainWindow mainWindow)
        {
            //initialize data
            Engine.Env.ThemeManager = new XAMLThemeController();
            MainWindowData = new MainWindowModel();

            EnvModel = new EnvModel();

            MainWindow = mainWindow;
        }

        public static void ExplicitFocus(TextBox element)
        {
            element.Dispatcher.BeginInvoke(DispatcherPriority.Input,
            new Action(delegate ()
            {
                element.Focus();         // Set Logical Focus
                Keyboard.Focus(element); // Set Keyboard Focus
                element.CaretIndex = element.Text.Length;
            }));
        }

        public static string GetHeaderImageFromUrl(string url)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.OptionReadEncoding = false;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    htmlDoc.Load(stream, Encoding.UTF8);
                }
            }
            var img = htmlDoc.DocumentNode.SelectSingleNode("//img");
            return img.Attributes["src"].Value;
        }
    }
}

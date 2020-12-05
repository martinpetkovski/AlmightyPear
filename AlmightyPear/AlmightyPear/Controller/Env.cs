using AlmightyPear.Model;
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

namespace AlmightyPear.Controller
{
    class Env
    {
        // global
        public static char PathSeparator = ':';
        public static string TempBinPath = "_TEMP";
        public static string ArchiveBinPath = "_ARCHIVE";
        public static int CharacterLimit = 5000;
        public static int BinPathCharacterLimit = 100;
        // data
        public static EnvModel EnvModel { get; private set; }
        public static MainWindowModel MainWindowData { get; private set; }
        public static UserModel UserData { get; private set; }
        public static BinMetaModel BinData { get; private set; }
        // functional
        public static FirebaseController FirebaseController { get; private set; }
        public static MainWindow MainWindow { get; private set; }
        public static MetroWindow MetroMainWindow { get { return (MetroWindow)MainWindow; } }
        public static BinController BinController { get; private set; }

        public static void Initialize(MainWindow mainWindow)
        {
            //initialize data

            MainWindowData = new MainWindowModel();
            UserData = new UserModel();
            BinData = new BinMetaModel();

            //initialize func
            FirebaseController = new FirebaseController();
            FirebaseController.Initialize();
            BinController = new BinController();

            EnvModel = new EnvModel();

            MainWindow = mainWindow;
        }

        private static int clipboardDelay = 10;
        // UTILS
        public static string GetClipboardText()
        {

            string clipboardData = null;
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        clipboardData = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.Text);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Thread.Sleep(clipboardDelay);

            return clipboardData;
        }

        public static void ClearClipboard()
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        System.Windows.Clipboard.Clear();
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            Thread.Sleep(clipboardDelay);
        }

        public static void CopyToClipboard(string text)
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        System.Windows.Clipboard.SetText(text);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();


            Thread.Sleep(clipboardDelay);
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

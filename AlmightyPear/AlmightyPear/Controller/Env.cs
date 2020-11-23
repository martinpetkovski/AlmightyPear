using AlmightyPear.Model;
using System;
using System.Threading;

namespace AlmightyPear.Controller
{
    class Env
    {
        // global
        public static char PathSeparator = ':';
        // data
        public static EnvModel EnvModel { get; private set; }
        public static MainWindowModel MainWindowData { get; private set; }
        public static UserModel UserData { get; private set; }
        public static BinMetaModel BinData { get; private set; }
        // functional
        public static FirebaseController FirebaseController { get; private set; }
        public static MainWindow MainWindow { get; private set; }
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


        // UTILS
        public static string GetClipboardText()
        {
            try
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
                return clipboardData;
            }
            catch (Exception exception)
            {
                return string.Empty;
            }
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
        }
    }
}

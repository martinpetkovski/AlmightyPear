using Checkmeg.WPF.Controller;
using Core;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace Checkmeg.WPF
{
    [Serializable]
    public class SerializableException
    {
        public byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public List<Exception> Exc;
        public SerializableException()
        {
            Exc = new List<Exception>();
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string CrashDumpPath = System.AppDomain.CurrentDomain.BaseDirectory + "crashdump.ccm";

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                if (e.Args[0] == "-s")
                {
                    Env.StartupState = WindowState.Minimized;
                }
            }
        }
    }

    public class SingleInstanceApplication : App
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            // Call the OnStartup event on our base class
            base.OnStartup(e);
            base.InitializeComponent();
        }

        public void Activate()
        {
            // Reactivate the main window
            MainWindow.Activate();
        }
    }

    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        public static SerializableException SerializableException;

        private SingleInstanceApplication _application;
        private System.Collections.ObjectModel.ReadOnlyCollection<string> _commandLine;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            SerializableException = new SerializableException();

            if (File.Exists(App.CrashDumpPath))
                File.Delete(App.CrashDumpPath);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                SerializableException.Exc.Add((Exception)e.ExceptionObject);
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                BinaryWriter writer = new BinaryWriter(File.Open(App.CrashDumpPath, FileMode.Append));
                writer.Write(SingleInstanceManager.SerializableException.Serialize());
                writer.Close();
            };

            // First time _application is launched
            _commandLine = eventArgs.CommandLine;
            _application = new SingleInstanceApplication();
            try
            {
                _application.Run();
            }
            catch (Exception) { }

            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            _commandLine = eventArgs.CommandLine;

            if (_commandLine.Count >= 2)
            {
                if (_commandLine[0] == "-a")
                {
                    Env.MainWindow.ShowHotWndCreateBookmarkAsync(null, new Utils.HotKeyEventArgs(_commandLine[1]));
                }
                else if (_commandLine[0] == "-f")
                {
                    Env.MainWindow.ShowHotWndFindBookmark(null, new Utils.HotKeyEventArgs(_commandLine[1]));
                }
                else if (_commandLine[0] == "-c")
                {
                    ClipboardManager.CopyToClipboard(_commandLine[1]);
                }
                else if (_commandLine[0] == "-cp")
                {
                    ClipboardManager.CopyToClipboard(Path.GetDirectoryName(_commandLine[1]));
                }
            }
            else
            {
                _application.Activate();
            }
        }
    }
}

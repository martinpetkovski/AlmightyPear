using Checkmeg.WPF.Controller;
using Checkmeg.WPF.View;
using Core;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Checkmeg.WPF
{
    

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                if (e.Args[0] == "-s")
                {
                    Env.StartupState = WindowState.Minimized;
                }
            }
            AppDomain.CurrentDomain.ProcessExit += (s, ex) => ExitHandler(s, ex);
        }

        private void ExitHandler(object sender, EventArgs e)
        {
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
        private SingleInstanceApplication _application;
        private System.Collections.ObjectModel.ReadOnlyCollection<string> _commandLine;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            // First time _application is launched
            _commandLine = eventArgs.CommandLine;
            _application = new SingleInstanceApplication();
            _application.Run();

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
                else if(_commandLine[0] == "-f")
                {
                    Env.MainWindow.ShowHotWndFindBookmark(null, new Utils.HotKeyEventArgs(_commandLine[1]));
                }
                else if(_commandLine[0] == "-c")
                {
                    ClipboardManager.CopyToClipboard(_commandLine[1]);
                }
                else if(_commandLine[0] == "-cp")
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

using Microsoft.Win32;
using System;
using System.IO;

namespace CheckmegWSC
{
    class Program
    {
        class RegistryEntry
        {
            private string path;
            private string name;
            private string icon;
            private string verb;
            private string command;
            private int flag;
            private bool isCascading;
            private bool separator;

            public RegistryEntry(bool casc, string path, string verb, string command, string icon = "", int flag = 3)
            {
                isCascading = true;
                this.path = path;
                this.verb = verb;
                this.command = command;
                this.flag = flag;
                this.icon = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\logo_icon.ico"; ;
            }

            public RegistryEntry(string path, string name, string verb, string command, string icon = "", int flag = 3, bool separator = false)
            {
                isCascading = false;
                this.path = path;
                this.name = name;
                this.flag = flag;
                this.icon = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\logo_icon.ico"; ;
                this.verb = verb;
                this.command = command;
                this.separator = separator;
            }

            

            private string GetFullPath()
            {
                return path + "\\" + name;
            }

            public void Create()
            {
                if (isCascading)
                {
                    RegistryKey regmenu = null;
                    RegistryKey regcmd = null;
                    try
                    {
                        regmenu = Registry.ClassesRoot.CreateSubKey(GetFullPath());
                        if (regmenu != null)
                        {
                            regmenu.SetValue("MUIVerb", verb);
                            regmenu.SetValue("subcommands", "");
                            if(icon != "")
                                regmenu.SetValue("Icon", icon);
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        if (regmenu != null)
                            regmenu.Close();
                        if (regcmd != null)
                            regcmd.Close();
                    }
                }
                else
                {
                    RegistryKey regmenu = null;
                    RegistryKey regcmd = null;
                    try
                    {
                        regmenu = Registry.ClassesRoot.CreateSubKey(GetFullPath());
                        if (regmenu != null)
                        {
                            regmenu.SetValue("", verb);
                            regmenu.SetValue("Icon", icon);
                            regmenu.SetValue("Position", flag == 0 ? "Bottom" : "Top" );
                            if (separator)
                                regmenu.SetValue("SeparatorAfter", "");
                        }
                        regcmd = Registry.ClassesRoot.CreateSubKey(GetFullPath() + "\\command");
                        if (regcmd != null)
                        {
                            string checkmegExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Checkmeg.exe";
                            regcmd.SetValue("", checkmegExePath + " " + command + " %1");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        if (regmenu != null)
                            regmenu.Close();
                        if (regcmd != null)
                            regcmd.Close();
                    }
                }
            }

            public void Delete()
            {
                if (isCascading)
                {
                    RegistryKey reg = Registry.ClassesRoot.OpenSubKey(GetFullPath());
                    if (reg != null)
                    {
                        reg.Close();
                        Registry.ClassesRoot.DeleteSubKey(GetFullPath());
                    }
                }
                else
                {
                    RegistryKey reg = Registry.ClassesRoot.OpenSubKey(GetFullPath());
                    if (reg != null)
                    {
                        reg.Close();
                        Registry.ClassesRoot.DeleteSubKey(GetFullPath() + "\\command");
                        Registry.ClassesRoot.DeleteSubKey(GetFullPath());
                    }
                }
            }
        }


        //private const string MenuName = "*\\shell\\Checkmeg";
        //private const string Command = "*\\shell\\Checkmeg\\command";

        private static string rootClass = @"*\shell";
        public static void CreateContextMenu()
        {
            RegistryEntry Root = new RegistryEntry(true, rootClass + "\\Checkmeg", "Checkmeg", "Checkmeg.AddBookmark", "", 0);
            Root.Create();

            RegistryEntry AddBookmark = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.AddBookmark", "Add Bookmark", "-a");
            AddBookmark.Create();

            RegistryEntry FindBookmark = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.FindBookmark", "Find Bookmark", "-f", "", 3, true);
            FindBookmark.Create();

            RegistryEntry CopyPath = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.zCopyPath", "Copy Path", "-c", "", 0);
            CopyPath.Create();

            RegistryEntry CopyParentDirPath = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.zzCopyParentDirPath", "Copy Dir Path", "-cp", "", 0);
            CopyParentDirPath.Create();
        }

        public static void DestroyContextMenu()
        {
            RegistryEntry Root = new RegistryEntry(true, rootClass + "\\Checkmeg", "Checkmeg", "Checkmeg.AddBookmark");
            Root.Delete();

            RegistryEntry AddBookmark = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.AddBookmark", "Add Bookmark", "-a");
            AddBookmark.Delete();

            RegistryEntry FindBookmark = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.FindBookmark", "Find Bookmark", "-f");
            FindBookmark.Delete();

            RegistryEntry CopyPath = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.zCopyPath", "Copy Path", "-c");
            CopyPath.Delete();

            RegistryEntry CopyParentDirPath = new RegistryEntry(rootClass + "\\Checkmeg\\shell", "Checkmeg.zzCopyParentDirPath", "Copy Dir Path", "-cp");
            CopyParentDirPath.Delete();
        }

        static void Configure()
        {
            CreateContextMenu();
        }

        static void Deconfigure()
        {
            DestroyContextMenu();
        }

        static void Main(string[] args)
        {
            if (args[0] == "-c")
            {
                Configure();
            }
            else if (args[0] == "-d")
            {
                Deconfigure();
            }
            else if (args[0] == "-t")
            {
                Console.WriteLine("Checkmeg (c) - Martin Petkovski, 2020 - 2021");
                if (args.Length > 1)
                {
                    Console.WriteLine(args[1]);
                }
            }
        }
    }
}

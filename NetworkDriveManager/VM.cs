using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using VBase;
using Wpf.Ui.Controls;
using File = System.IO.File;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace NetworkDriveManager
{
    public class VM:ViewModel
    {
        const int VERSION = 2;
        FileBrowser browser = new FileBrowser(".xml", "XML file");
        Timer timer = new Timer(1000*60*60*4);
        public VM()
        {
            timer.Elapsed +=Timer_Elapsed;
            timer.Start();
            CheckForUpdates();

            EditClose = new RelayCommand(o=> SaveChanges(o));
            Add = new RelayCommand(_ => AddDrive());
            Exit = new RelayCommand(_ => ExitProgram());
            Export = new RelayCommand(_ => export());
            Import = new RelayCommand(_ => import());

            //IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            //string shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\NetDriveMgmt.lnk";
            //System.Reflection.Assembly curAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            //IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutAddress);
            //shortcut.Description = "Netword drive manager";
            //shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //shortcut.TargetPath = curAssembly.Location.Replace("dll","");
            //shortcut.IconLocation = AppDomain.CurrentDomain.BaseDirectory + @"Network_drive.ico";
            //shortcut.Save();

            Drives = new();
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mukunya/NetworkDriveMan/drives.xml")))
            {
                XmlSerializer xml = new(typeof(Drive[]));
                FileStream s = File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mukunya/NetworkDriveMan/drives.xml"));
                try
                {
                    Drive[] drives = xml.Deserialize(s) as Drive[];
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var item in drives)
                        {
                            item.DeleteDrive += D_DeleteDrive;
                            item.EditDrive += D_EditDrive;
                            item.SaveDrives +=Save;
                            Drives.Add(item);
                        }
                    });
                }
                catch
                {

                }
                s.Close();
                
            }

        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string s = client.DownloadString("https://raw.githubusercontent.com/Mukunya/NetDriveMan/master/version.txt");
                    if (int.Parse(s)>VERSION)
                    {
                        var r = MessageBox.Show("A new version of the network drive manager is available. Would you like to open the releases page?", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (r.HasFlag(MessageBoxResult.Yes))
                        {
                            Process.Start(new ProcessStartInfo("https://github.com/Mukunya/NetDriveMan/releases") { UseShellExecute = true });
                        }
                    }
                }
            }
            catch
            {

            }
            
        }

        private void Save(object? sender, EventArgs e)
        {
            SaveChanges(null);
        }

        public ObservableCollection<Drive> Drives { get; set; }
        public ICommand EditClose { get; set; }
        public ICommand Add { get; set; }
        public ICommand Exit { get; set; }
        public ICommand Export { get; set; }
        public ICommand Import { get; set; }
        private bool editing = false;
        public bool Editing
        {
            get => editing;
            set
            {
                editing = value;
                OnPropertyChanged("Editing");
            }
        }
        private Drive editingdrive;
        public Drive EditingDrive
        {
            get=> editingdrive;
            private set
            {
                editingdrive = value;
                OnPropertyChanged("EditingDrive");
            }
        }
        
        private void SaveChanges(object o)
        {
            if (o!=null)
            {
                PasswordBox pwd = o as PasswordBox;
                string password = "";
                App.Current.Dispatcher.Invoke(() =>
                {
                    password = pwd.Password;
                });
                if (password != "")
                {
                    EditingDrive.Password = password;
                }
                App.Current.Dispatcher.Invoke(() =>
                {
                    pwd.Password = "";
                });
            }
            Editing = false;
            XmlSerializer xml = new(typeof(Drive[]));
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Mukunya/NetworkDriveMan")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mukunya/NetworkDriveMan"));
            }
            FileStream s = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mukunya/NetworkDriveMan/drives.xml"));
            xml.Serialize(s, Drives.ToArray());
            s.Close();
        }
        public void ExitProgram()
        {
            foreach (var item in Drives)
            {
                item.Remove();
            }
            Environment.Exit(0);
        }
        private void AddDrive()
        {
            Drive d = new Drive();
            d.EditDrive += D_EditDrive;
            d.DeleteDrive += D_DeleteDrive;
            d.SaveDrives += Save;
            App.Current.Dispatcher.Invoke(() =>
            {
                Drives.Add(d);
            });
            EditingDrive = d;
            Editing = true;
        }

        private void D_DeleteDrive(object? sender, Drive e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Drives.Remove(e);
            });
            SaveChanges(null);
        }

        private void D_EditDrive(object? sender, Drive e)
        {
            EditingDrive = e;
            Editing = true;
        }
        private void export()
        {
            string fname = browser.GetFileName(FileBrowser.EFileOperations.Save);
            if (fname == "")
                return;
            XmlSerializer xml = new(typeof(Drive[]));
            FileStream s = File.Create(fname);
            xml.Serialize(s, Drives.Where(o=>o.Enabled).ToArray());
            s.Close();

        }
        private void import()
        {
            try
            {
                string fname = browser.GetFileName(FileBrowser.EFileOperations.Open);
                if (fname == "")
                    return;
                XmlSerializer xml = new(typeof(Drive[]));
                FileStream s = File.OpenRead(fname);
                Drive[] a = xml.Deserialize(s) as Drive[];
                s.Close();
                foreach (var item in a)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Drives.Add(item);
                    });
                }
                SaveChanges(null);
            }
            catch
            {

            }

        }
    }
}

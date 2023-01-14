using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using VBase;
using Wpf.Ui.Controls;

namespace NetworkDriveManager
{
    public class VM:ViewModel
    {
        public static VM Instance = new VM();
        public VM()
        {
            EditClose = new RelayCommand(o=> SaveChanges(o));
            Add = new RelayCommand(_ => AddDrive());
            Exit = new RelayCommand(_ => ExitProgram());
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

        private void Save(object? sender, EventArgs e)
        {
            SaveChanges(null);
        }

        public ObservableCollection<Drive> Drives { get; set; }
        public ICommand EditClose { get; set; }
        public ICommand Add { get; set; }
        public ICommand Exit { get; set; }
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
    }
}

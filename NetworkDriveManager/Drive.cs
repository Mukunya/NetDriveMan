using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Input;
using VBase;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace NetworkDriveManager
{
    
    public class Drive : ViewModel
    {
        static WshNetwork Net = new WshNetwork();
        public enum Status
        {
            Working, Disabled, Unreachable, AuthenticationError, OtherError
        }
        public static string[] DriveLetters = new[]
        {
            "A:","B:","C:","D:","E:","F:","G:","H:","I:","J:","K:","L:","M:","N:","O:","P:","Q:","R:","S:","T:","U:","V:","W:","X:","Y:","Z:"
        };
        [XmlIgnore]
        public ObservableCollection<string> AvailableLetters { get; set; } = new ObservableCollection<string>();
        [XmlIgnore]
        private Status status = Status.Disabled;
        [XmlIgnore]
        public Status DriveState
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged("Working");
                OnPropertyChanged("Disabled");
                OnPropertyChanged("Unreachable");
                OnPropertyChanged("AuthenticationError");
                OnPropertyChanged("OtherError");
            }
        }
        [XmlIgnore]
        public bool Working { get => DriveState== Status.Working; }
        [XmlIgnore]
        public bool Disabled { get => DriveState== Status.Disabled; }
        [XmlIgnore]
        public bool Unreachable { get => DriveState== Status.Unreachable; }
        [XmlIgnore]
        public bool AuthenticationError { get => DriveState== Status.AuthenticationError; }
        [XmlIgnore]
        public bool OtherError { get => DriveState== Status.OtherError; }
        private string name = "";
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }
        private string letter = "";
        public string Letter
        {
            get { return letter; }
            set 
            {
                if (value != null && value != "")
                {
                    letter = value; OnPropertyChanged("Letter");
                }
            }
        }
        private bool enabled = false;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; OnPropertyChanged("Enabled"); Check(); SaveDrives?.Invoke(this, null); }
        }
        private string address = "";
        public string Address
        {
            get { return address; }
            set { address = value; OnPropertyChanged("Address"); }
        }
        private string user = "";
        public string User
        {
            get { return user; }
            set { user = value; OnPropertyChanged("User"); }
        }
        private string pwd = "";
        public string Password
        {
            get { return pwd; }
            set { pwd = value; OnPropertyChanged("Password"); }
        }
        [XmlIgnore]
        public ICommand Delete { get => delete1; set => delete1=value; }
        [XmlIgnore]
        public ICommand Edit { get => edit1; set => edit1=value; }

        private Timer timer = new Timer(1000*60);
        [XmlIgnore]
        private ICommand delete1;
        [XmlIgnore]
        private ICommand edit1;

        public Drive()
        {
            Delete = new RelayCommand(o => delete());
            Edit = new RelayCommand(o => edit());
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Check();
        }

        public event EventHandler<Drive> EditDrive;
        public event EventHandler<Drive> DeleteDrive;
        public event EventHandler SaveDrives;
        public void edit()
        {
            Enabled = false;
            App.Current.Dispatcher.Invoke(() =>
            {
                AvailableLetters.Clear();
                foreach (var item in DriveLetters)
                {
                    AvailableLetters.Add(item);
                }
                foreach (var item in Environment.GetLogicalDrives())
                {
                    AvailableLetters.Remove(item.Substring(0, 2));
                }
                if (Letter == "")
                {
                    Letter = AvailableLetters.First();
                }
                else
                {
                    AvailableLetters.Add(Letter);
                }
            });
            EditDrive?.Invoke(this, this);
        }
        public void delete()
        {
            Remove();
            DeleteDrive?.Invoke(this, this);
        }
        public void Remove()
        {
            try
            {
                Net.RemoveNetworkDrive(Letter);
            }
            catch
            {
            }
        }
        static string[] GetMappedNetworkDrives()
        {
            IWshCollection colNetDrives = Net.EnumNetworkDrives();
            var enumerator = colNetDrives.GetEnumerator();
            var rll = new List<string>();
            while (enumerator.MoveNext())
            {

                string item = enumerator.Current as string;
                if (item == "")
                {
                    enumerator.MoveNext();
                    continue;
                }
                enumerator.MoveNext();
                rll.Add(item);
            }

            return rll.ToArray();
        }
        private void Register()
        {
            try
            {
                Net.MapNetworkDrive(Letter, Address, "false", User, Password);
                
                DriveState = Status.Working;
            }
            catch (Exception ex)
            {
                switch (ex.HResult)
                {
                    case -2147024810:
                    case -2147024891:
                    case -2147023570:
                        DriveState = Status.AuthenticationError;
                        break;

                    case -2147024829:
                    case -2147024865:
                    default:
                        DriveState = Status.OtherError;
                        break;
                }
                Remove();
            }
        }
        public void Check()
        {
            Task.Run(() =>
            {
                if (Enabled)
                {
                    Ping ping = new Ping();
                    string hostname = "";
                    if (Address.Length<2)
                    {
                        DriveState = Status.Disabled;
                        return;
                    }
                    if (Address.Contains('@'))
                    {
                        hostname = Address.Substring(2).Split('@')[1].Split('\\')[0];
                    }
                    else
                    {
                        hostname = Address.Substring(2).Split('\\')[0];
                    }
                    try
                    {
                        if (!(GetMappedNetworkDrives().Contains(Letter)) && DriveState == Status.Working)
                        {
                            DriveState = Status.Unreachable;
                        }
                        var r = ping.Send(hostname,500);
                        if (r.Status == IPStatus.Success)
                        {
                            if (DriveState != Status.Working)
                            {
                                Remove();
                                Register();
                            }
                        }
                        else
                        {
                            Remove();
                            DriveState = Status.Unreachable;
                        }
                    }
                    catch
                    {
                        if (DriveState == Status.Working)
                        {
                            Remove();
                        }
                        DriveState = Status.Unreachable;
                    }
                    
                }
                else
                {
                    Remove();
                    DriveState = Status.Disabled;
                }
            });
            
        }

    }
}

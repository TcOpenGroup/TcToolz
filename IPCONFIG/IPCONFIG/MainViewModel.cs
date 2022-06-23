using IPCONFIG.DataAccess;
using IPCONFIG.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Updater;

namespace IPCONFIG
{
    public class MainViewModel : Observable
    {
        private ItemWrapper _selectedItem;
        public ItemWrapper SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private InterfaceWrapper _selectedInterface;
        public InterfaceWrapper SelectedInterface
        {
            get { return _selectedInterface; }
            set
            {
                _selectedInterface = value;
                OnPropertyChanged(nameof(SelectedInterface));
                OnSelectedIterface();
            }
        }

        private InterfaceWrapper _selectedAdapter;
        public InterfaceWrapper SelectedAdapter
        {
            get { return _selectedAdapter; }
            set
            {
                _selectedAdapter = value;
                OnPropertyChanged(nameof(SelectedAdapter));
                OnSelectedAdapter();
            }
        }

        public int SelectedIndex { get; set; }
        public int SelectedInterfaceIndex { get; set; }
        public int SelectedAdapterIndex { get; set; }

        private ObservableCollection<ItemWrapper> _items;
        public ObservableCollection<ItemWrapper> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private ObservableCollection<InterfaceWrapper> _interfaces;
        public ObservableCollection<InterfaceWrapper> Interfaces
        {
            get { return _interfaces; }
            set
            {
                _interfaces = value;
                OnPropertyChanged(nameof(Interfaces));
            }
        }

        private ObservableCollection<InterfaceWrapper> _adapters;
        public ObservableCollection<InterfaceWrapper> Adapters
        {
            get { return _adapters; }
            set
            {
                _adapters = value;
                OnPropertyChanged(nameof(Adapters));
            }
        }

        private string _ip;
        public string IP
        {
            get { return _ip; }
            set
            {
                _ip = value;
                OnPropertyChanged(nameof(IP));
            }
        }

        private string _subnetMask = "255.255.255.0";
        public string SubnetMask
        {
            get { return _subnetMask; }
            set
            {
                _subnetMask = value;
                OnPropertyChanged(nameof(SubnetMask));
            }
        }

        private string _ipToPing;
        public string IPToPing
        {
            get { return _ipToPing; }
            set
            {
                _ipToPing = value;
                OnPropertyChanged(nameof(IPToPing));
            }
        }

        private bool _t_PingParam;
        public bool T_PingParam
        {
            get { return _t_PingParam; }
            set
            {
                _t_PingParam = value;
                OnPropertyChanged(nameof(T_PingParam));
            }
        }

        private string _groupBox1Header;
        public string GroupBox1Header
        {
            get { return _groupBox1Header; }
            set
            {
                _groupBox1Header = value;
                OnPropertyChanged(nameof(GroupBox1Header));
            }
        }

        private bool _groupBox1Enabled;
        public bool GroupBox1Enabled
        {
            get { return _groupBox1Enabled; }
            set
            {
                _groupBox1Enabled = value;
                OnPropertyChanged(nameof(GroupBox1Enabled));
            }
        }

        private bool _radioButton1Selected;
        public bool RadioButton1Selected
        {
            get { return _radioButton1Selected; }
            set
            {
                _radioButton1Selected = value;
                OK = false;
                OnPropertyChanged(nameof(RadioButton1Selected));
            }
        }

        private bool _radioButton2Selected;
        public bool RadioButton2Selected
        {
            get { return _radioButton2Selected; }
            set
            {
                _radioButton2Selected = value;
                OK = false;
                OnPropertyChanged(nameof(RadioButton2Selected));
            }
        }

        private bool _ok;
        public bool OK
        {
            get { return _ok; }
            set
            {
                _ok = value;
                OnPropertyChanged(nameof(OK));
            }
        }


        private readonly IDataService _dataService;

        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteAllCommand { get; private set; }
        public ICommand DuplicateCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand SetCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand SetSpecificIPCommand { get; private set; }
        public ICommand PingCommand { get; private set; }

        public MainViewModel()
        {
            UpdaterInterface.Instance.Run(new UpdaterData
            {
                VersionFileUrl = "http://192.168.0.14:5551/IPCONFIG/version.txt",
                SourceFileUrl = "http://192.168.0.14:5551/IPCONFIG/IPCONFIG_SETUP.exe",
                TmpFilePath = Path.GetTempPath() + @"IPCONFIG\IPCONFIG_SETUP.exe",
                CurrentVersion = Utils.GetVersion(),
            });

            _dataService = new FileDataService();

            _items = new ObservableCollection<ItemWrapper>();
            _view = new ListCollectionView(_items);

            _interfaces = new ObservableCollection<InterfaceWrapper>();
            _adapters = new ObservableCollection<InterfaceWrapper>();

            GetAllProfiles();

            if (_items.Count() < 1)
            {
                _items.Add(new ItemWrapper(new Item
                {
                    Name = "Varroc 2100735",
                    IP = "10.0.225.199",
                    SubnetMask = "255.255.255.0",
                }));

                GenId();
            }

            Sort();

            Interfaces.Add(new InterfaceWrapper(new Interface
            {
                Name = "Ethernet",
            }));
            Interfaces.Add(new InterfaceWrapper(new Interface
            {
                Name = "Wireless",
            }));
            SelectedInterface = Interfaces[0];

            ReloadAdapters();


            AddCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    var d = new AddWindow();
                    d.ShowDialog();
                    if (d.Save)
                    {
                        _items.Add(d.Item);
                        SaveAll();
                        SelectedItem = d.Item;
                    }
                }
                catch (Exception ex)
                {
                }
            });

            DeleteCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    if (SelectedItem == null)
                    {
                        return;
                    }
                    var res = MessageBox.Show("Delete?", Application.ResourceAssembly.GetName().Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (res != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    var selectedIndex = SelectedIndex;
                    _items.Remove(SelectedItem);
                    //UpdateListSelection(_items.Count, selectedIndex);
                    SaveAll();
                }
                catch (Exception ex)
                {
                }
            });

            DeleteAllCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    var res = MessageBox.Show("Delete All?", Application.ResourceAssembly.GetName().Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (res != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    _items.Clear();
                    SaveAll();
                }
                catch (Exception ex)
                {
                }
            });

            DuplicateCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    if (SelectedItem == null)
                    {
                        return;
                    }
                    var newitem = Utils.Clone(SelectedItem);
                    _items.Add(newitem);

                    SaveAll();
                    SelectedItem = newitem;
                }
                catch (Exception ex)
                {
                }
            });

            SaveCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    SaveAll();
                }
                catch (Exception ex)
                {
                }
            });

            EditCommand = new DelegateCommand((obj) =>
             {
                 try
                 {
                     if (obj == null)
                     {
                         return;
                     }

                     SelectedItem = obj as ItemWrapper;

                     var d = new EditWindow(Utils.Clone(obj as ItemWrapper));
                     d.ShowDialog();
                     ItemWrapper item1 = null;
                     if (d.Item != null)
                     {
                         foreach (var item in _items)
                         {
                             if (item.Id == d.Item.Id)
                             {
                                 var y = _items.Where(x => x.Id == d.Item.Id).SingleOrDefault();
                                 _items[_items.IndexOf(y)] = d.Item;
                                 item1 = d.Item;

                                 SaveAll();
                                 SelectedItem = item1;
                                 break;
                             }
                         }
                     }
                 }
                 catch (Exception ex)
                 {

                 }
             });

            SetCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    SelectedItem = obj as ItemWrapper;

                    var item = obj as ItemWrapper;
                    SetIP(true, item.Model);
                    OnSelectedAdapter();
                    OK = true;
                }
                catch (Exception ex)
                {

                }
            });

            ExportCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "IPCONFIG";
                    dlg.DefaultExt = ".json";
                    dlg.Filter = "JSON Files (.json)|*.json";
                    var result = dlg.ShowDialog();

                    if (result == true)
                    {
                        File.Copy(_dataService.FilePath(), dlg.FileName, true);
                    }
                }
                catch (Exception ex)
                {

                }
            });

            ImportCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
                    openFileDlg.DefaultExt = ".json";
                    openFileDlg.Filter = "JSON Files (.json)|*.json";

                    Nullable<bool> result = openFileDlg.ShowDialog();
                    if (result == true)
                    {
                        File.Copy(openFileDlg.FileName, _dataService.FilePath(), true);
                        GetAllProfiles();
                        Sort();
                    }
                }
                catch (Exception ex)
                {

                }
            });

            SetSpecificIPCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    SetIP(RadioButton2Selected, new Item { IP = IP, SubnetMask = SubnetMask });
                }
                catch (Exception ex)
                {

                }
            });

            PingCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    startInfo.FileName = "cmd.exe";
                    if (T_PingParam)
                    {
                        startInfo.Arguments = "/c ping " + IPToPing + " -t";
                    }
                    else
                    {
                        startInfo.Arguments = "/c ping " + IPToPing;
                    }
                    process.StartInfo = startInfo;
                    process.Start();
                }
                catch (Exception ex)
                {

                }
            });
        }

        private void OnSelectedIterface()
        {
            OK = false;
            ReloadAdapters();
        }

        public string AdapterId { get; set; }

        private void OnSelectedAdapter()
        {
            if (SelectedAdapter == null)
            {
                return;
            }
            GroupBox1Enabled = false;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                {
                    if (nic.Description == SelectedAdapter.Name)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            GroupBox1Enabled = true;
                            GroupBox1Header = nic.Name;
                            AdapterId = nic.Id;
                            IP = ip.Address.ToString();
                        }
                    }
                }
            }
            var objMC = new ManagementObjectSearcher("Select * from Win32_NetworkAdapterConfiguration where SettingID=\"" + AdapterId + "\"");
            var objMOC = objMC.Get();
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["DHCPEnabled"])
                {
                    RadioButton1Selected = true;
                    RadioButton2Selected = false;
                }
                else
                {
                    RadioButton1Selected = false;
                    RadioButton2Selected = true;
                }
            }
        }

        public static ObservableCollection<ItemWrapper> OrderGroups(ObservableCollection<ItemWrapper> orderThoseGroups)
        {
            try
            {
                ObservableCollection<ItemWrapper> temp;
                temp = new ObservableCollection<ItemWrapper>(orderThoseGroups.OrderBy(p => p.Name));
                orderThoseGroups.Clear();
                foreach (ItemWrapper j in temp) orderThoseGroups.Add(j);
            }
            catch (Exception)
            {
            }
            return orderThoseGroups;
        }

        private void ReloadAdapters()
        {
            _adapters.Clear();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (SelectedInterfaceIndex == 0)
                {
                    if ((nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || (int)nic.NetworkInterfaceType == 53) && nic.OperationalStatus == OperationalStatus.Up)
                    {
                        _adapters.Add(new InterfaceWrapper(new Interface
                        {
                            Name = nic.Description,
                        }));
                    }
                }
                else
                {
                    if ((nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && (nic.OperationalStatus == OperationalStatus.Up))
                    {
                        _adapters.Add(new InterfaceWrapper(new Interface
                        {
                            Name = nic.Description,
                        }));
                    }
                }
            }
            if (_adapters.Count > 0)
            {
                SelectedAdapter = _adapters[0];
            }
        }

        private void SetIP(bool staticIP, Item item)
        {
            OK = false;
            var query = new ManagementObjectSearcher("Select * from Win32_NetworkAdapterConfiguration where SettingID=\"" + AdapterId + "\"");
            var moc = query.Get();
            foreach (ManagementObject mo in moc)
            {
                ManagementBaseObject setIP;
                ManagementBaseObject newIP = mo.GetMethodParameters("EnableStatic");
                if (!staticIP)
                {
                    setIP = mo.InvokeMethod("EnableDHCP", null, null);
                }
                else
                {
                    newIP["IPAddress"] = new string[] { item.IP };
                    newIP["SubnetMask"] = new string[] { item.SubnetMask };
                    mo.InvokeMethod("EnableStatic", newIP, null);
                    mo.Dispose();

                    string[] IPAddress = null;
                    string[] subMask = null;
                    IPAddress = (string[])mo["IPAddress"];
                    subMask = (string[])mo["IPSubnet"];
                }
                OK = true;
            }
        }


        private void GetAllProfiles()
        {
            _items.Clear();
            var items = _dataService.GetItems();
            foreach (var item in items)
            {
                Items.Add(new ItemWrapper(item));
            }
        }

        private void Sort()
        {
            _items = OrderGroups(_items);
        }

        private ListCollectionView _view;
        public ICollectionView View
        {
            get { return this._view; }
        }


        private string _textSearch;
        public string TextSearch
        {
            get { return _textSearch; }
            set
            {
                _textSearch = value;
                OnPropertyChanged(nameof(TextSearch));

                if (String.IsNullOrEmpty(value))
                {
                    View.Filter = null;
                }
                else
                {
                    try
                    {
                        View.Filter = new Predicate<object>(o =>
                        ((ItemWrapper)o).Name.ToLower().Contains(value.ToLower()));
                        SelectedItem = (ItemWrapper)View.CurrentItem;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void SaveAll()
        {
            GenId();
            Sort();
            var items = Items.Select(x => x.Model).ToList();
            _dataService.SaveItems(items);
        }

        private void GenId()
        {
            var i = 1;
            foreach (var item in _items)
            {
                item.Id = i;
                i++;
            }
        }

        //private void UpdateListSelection(int count, int selectedIndex)
        //{
        //    try
        //    {
        //        if (count == 0)
        //        {
        //            SelectedItem = null;
        //        }
        //        else if (count <= selectedIndex)
        //        {
        //            SelectedItem = _items[count - 1];
        //        }
        //        else
        //        {
        //            SelectedItem = _items[selectedIndex];
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
    }

    public class OkIconVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else
            {

                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

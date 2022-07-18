using Newtonsoft.Json;
using QuickConnect.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Updater;

namespace QuickConnect
{
    public class MainViewModel : Observable
    {
        public static MainViewModel Instance;

        const string Quote = "\"";

        private SettingsWrapper _settings;
        public SettingsWrapper Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private ItemWrapper _selectedItem;
        public ItemWrapper SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                InvalidateCommands();
            }
        }


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

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged(nameof(Loading));
            }
        }

        private bool _closeOnSubmit;
        public bool CloseOnSubmit
        {
            get { return _closeOnSubmit; }
            set
            {
                _closeOnSubmit = value;
                OnPropertyChanged(nameof(CloseOnSubmit));
            }
        }

        private IDataService _dataService;

        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteAllCommand { get; private set; }
        public ICommand DuplicateCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand ExportAllCommand { get; private set; }
        public ICommand ImportAllCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }
        public ICommand KillCommand { get; private set; }

        void Initialize()
        {
            Instance = this;

            UpdaterInterface.Instance.Run(new UpdaterData
            {
                VersionFileUrl = "http://192.168.0.14:5551/TeamViewerQuickConnect/version.txt",
                SourceFileUrl = "http://192.168.0.14:5551/TeamViewerQuickConnect/TeamViewerQuickConnect_SETUP.exe",
                TmpFilePath = Path.GetTempPath() + @"TeamViewerQuickConnect\TeamViewerQuickConnect_SETUP.exe",
                CurrentVersion = Utils.GetVersion(),
            });

            _dataService = new FileDataService();
            _items = new ObservableCollection<ItemWrapper>();
            _view = new ListCollectionView(_items);


            //var dummyItems = new List<Item>();
            //for (int i = 1; i < 100000; i++)
            //{
            //    dummyItems.Add(new Item() { Name = "Item" + i.ToString(), TeamViewerID = "1.0.0.0", Password = "123" });
            //}
            //_dataService.AddRange(dummyItems);


            Settings = new SettingsWrapper(new Settings());

            try
            {
                Settings = new SettingsWrapper(_dataService.GetSettings());
            }
            catch (TeamViewerNotFoundException ex)
            {
                MessageBox.Show("TeamViewer is not installed on your computer", Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
            }


            Application.Current.Dispatcher.Invoke(() =>
            {
                ChangeTheme();
            });

            Loading = true;

            Task.Factory.StartNew(() =>
            {
                _dataService.EnsureDbPathExists();

                using (var client = new DataContext())
                {
                    client.Database.EnsureCreated();
                }

                _dataService.MigrateFromXml();

                var items = GetAllItemsParallel();



                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }

                    if (_items.Count < 1)
                    {

                        _items.Add(new ItemWrapper(new Item
                        {
                            Name = "2100735 OP01",
                            TeamViewerID = "10.0.225.10",
                            Password = "Password",
                        }));
                    }

                    Loading = false;

                    if (!string.IsNullOrEmpty(Settings.LastQuery))
                    {
                        TextSearch = Settings.LastQuery;
                        MainWindow.Instance.TextSearch1.SelectAll();
                    }
                });
            });
        }

        System.Timers.Timer _delayTimer = new System.Timers.Timer();
        public MainViewModel()
        {
            Initialize();

            _delayTimer.Interval = 5000;

            AddCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    var d = new AddWindow();
                    d.ShowDialog();
                    if (d.Save)
                    {
                        _dataService.Add(d.Item.Model);
                        GetAllItems();
                    }
                });
            });

            DeleteCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    var selected = MainWindow.Instance.ListBox.SelectedItems.Cast<ItemWrapper>().ToList();
                    if (selected == null || selected.Count == 0)
                    {
                        return;
                    }

                    var items = new List<Item>();

                    foreach (var item in selected)
                    {
                        items.Add(item.Model);
                    }

                    _dataService.RemoveRange(items);
                    GetAllItems();
                });

            }, (val) => SelectedItem != null);

            DeleteAllCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    var res = MessageBox.Show("Delete All?", Application.ResourceAssembly.GetName().Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (res != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    _dataService.RemoveAll();
                    GetAllItems();
                });
            });

            DuplicateCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    if (SelectedItem == null)
                    {
                        return;
                    }
                    var newitem = Utils.Clone(SelectedItem);

                    _dataService.Add(newitem.Model);

                    GetAllItems();

                    SelectedItem = newitem;
                });
            }, (val) => SelectedItem != null);

            EditCommand = new DelegateCommand((obj) =>
             {
                 Utils.RunWithErrorHandling(() =>
                 {
                     if (obj == null)
                     {
                         return;
                     }

                     SelectedItem = obj as ItemWrapper;

                     var d = new EditWindow(Utils.Clone(obj as ItemWrapper));
                     d.ShowDialog();

                     if (d.Item == null)
                     {
                         return;
                     }

                     _dataService.Update(d.Item.Model);

                     GetAllItems();

                     if (d.Item != null)
                     {
                         foreach (var item in _items)
                         {
                             if (item.Id == d.Item.Id)
                             {
                                 var y = _items.Where(x => x.Id == d.Item.Id).SingleOrDefault();
                                 _items[_items.IndexOf(y)] = d.Item;

                                 var filteredItems = View.Cast<ItemWrapper>();
                                 SelectedItem = filteredItems.Where(x => x.Id == item.Id).FirstOrDefault();

                                 break;
                             }
                         }
                     }
                 });
             });

            ExportCommand = new DelegateCommand((obj) =>
            {

                Utils.RunWithErrorHandling(() =>
                {
                    var selected = MainWindow.Instance.ListBox.SelectedItems.Cast<ItemWrapper>().ToList();
                    if (selected == null || selected.Count == 0)
                    {
                        return;
                    }

                    var items = new List<Item>();
                    foreach (var item in selected)
                    {
                        items.Add(item.Model);
                    }

                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "QuickConnect";
                    dlg.DefaultExt = ".json";
                    dlg.Filter = "JSON Files (.json)|*.json";
                    var result = dlg.ShowDialog();

                    if (result == true)
                    {
                        _dataService.ExportToXML(dlg.FileName, items);
                    }
                });
            });

            ImportCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
                    openFileDlg.DefaultExt = ".json";
                    openFileDlg.Filter = "JSON Files (.json)|*.json";

                    Nullable<bool> result = openFileDlg.ShowDialog();
                    if (result == true)
                    {
                        _dataService.ImportFromXML(openFileDlg.FileName);
                        GetAllItems();
                    }
                });
            });

            ExportAllCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "QuickConnect";
                    dlg.DefaultExt = ".db";
                    dlg.Filter = "SQL Files (.db)|*.db";
                    var result = dlg.ShowDialog();

                    if (result == true)
                    {
                        File.Copy(_dataService.GetItemsFilePath(), dlg.FileName, true);
                    }
                });
            });

            ImportAllCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
                    openFileDlg.DefaultExt = ".db";
                    openFileDlg.Filter = "SQL Files (.db)|*.db";

                    Nullable<bool> result = openFileDlg.ShowDialog();
                    if (result == true)
                    {
                        File.Copy(openFileDlg.FileName, _dataService.GetItemsFilePath(), true);
                        GetAllItems();
                    }
                });
            });

            SettingsCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    var d = new SettingsWindow(Settings);
                    d.ShowDialog();
                    Settings = Utils.Clone(d.DataContext as SettingsWrapper);
                    SaveSettings();
                });
            });

            KillCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    foreach (var process in Process.GetProcessesByName("teamviewer"))
                    {
                        process.Kill();
                    }
                });
            });
        }


        private void GetAllItems()
        {
            Utils.RunWithErrorHandling(() =>
            {
                Items.Clear();

                IEnumerable<Item> items = null;
                if (string.IsNullOrEmpty(TextSearch))
                {
                    items = _dataService.GetItems();
                }
                else
                {
                    items = _dataService.FindByName(TextSearch);
                }

                foreach (var item in items)
                {
                    Items.Add(new ItemWrapper(item));
                }
            });
        }

        private ObservableCollection<ItemWrapper> GetAllItemsParallel()
        {
            var result = new ObservableCollection<ItemWrapper>();
            Utils.RunWithErrorHandling(() =>
            {
                IEnumerable<Item> items = null;
                if (string.IsNullOrEmpty(Settings.LastQuery))
                {
                    items = _dataService.GetItems();
                }
                else
                {
                    items = _dataService.FindByName(Settings.LastQuery);
                }
                foreach (var item in items)
                {
                    result.Add(new ItemWrapper(item));
                }
            });

            return result;
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
                    Items.Clear();
                    GetAllItems();
                }
                else
                {
                    try
                    {
                        GetAllItems();
                        View.Filter = new Predicate<object>(o =>
                        ((ItemWrapper)o).Name.ToLower().Contains(TextSearch.ToLower()));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }


        public void Run()
        {
            if (SelectedItem == null)
            {
                return;
            }

            Debug.WriteLine(SelectedItem.Name);
            //return;

            ProcessStartInfo processInfo;
            Process process;
            processInfo = new ProcessStartInfo("cmd.exe");
            processInfo.Arguments = "/c " + Quote + Settings.TeamViewerPath + Quote + " -i " + SelectedItem.TeamViewerID + " -p " + SelectedItem.Password;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.Close();
        }

        public void RunFitered()
        {
            if (View.IsEmpty)
            {
                return;
            }
            else
            {
                var filteredItems = View.Cast<ItemWrapper>();
                SelectedItem = filteredItems.FirstOrDefault();
            }

            Run();
        }

        public void SaveSettings()
        {
            Settings.Model.LastQuery = TextSearch;
            _dataService.SaveSettings(Settings.Model);
            ChangeTheme();
        }

        public void ChangeTheme()
        {
            if (Settings.Theme == AppTheme.Light)
            {
                ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Light;
            }
            else if (Settings.Theme == AppTheme.Dark)
            {
                ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;
            }
            else
            {
                ModernWpf.ThemeManager.Current.ApplicationTheme = null;
            }
        }

        public void InvalidateCommands()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((DelegateCommand)AddCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)DuplicateCommand).RaiseCanExecuteChanged();
            });
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
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

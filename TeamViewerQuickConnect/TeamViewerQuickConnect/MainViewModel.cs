using Newtonsoft.Json;
using QuickConnect.DataAccess;
using QuickConnect.Model;
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
        public ICommand SettingsCommand { get; private set; }

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


            Settings = new SettingsWrapper(new Settings());

            Task.Run(() =>
            {
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    GetAllItems();

                    if (_items.Count < 1)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _items.Add(new ItemWrapper(new Item
                            {
                                Name = "2100735 OP01",
                                ID = "10.0.225.10",
                                Password = "MTSservis2100735",
                            }));

                            GenId();
                        });
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Sort();
                    });
                });
            });
        }

        public MainViewModel()
        {
            Initialize();

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
                    var res = MessageBox.Show("Delete " + SelectedItem.Name + " ?", Application.ResourceAssembly.GetName().Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (res != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    _items.Remove(SelectedItem);
                    SaveAll();
                }
                catch (Exception ex)
                {
                }
            }, (val) => SelectedItem != null);

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
            }, (val) => SelectedItem != null);

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

            ExportCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = "QuickConnect";
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
                        GetAllItems();
                        Sort();
                    }
                }
                catch (Exception ex)
                {

                }
            });

            SettingsCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    var d = new SettingsWindow(Settings);
                    d.ShowDialog();
                    Settings = Utils.Clone(d.DataContext as SettingsWrapper);
                    SaveSettings();
                }
                catch (Exception ex)
                {

                }
            });
        }

        public static ObservableCollection<ItemWrapper> ReorderItems(ObservableCollection<ItemWrapper> orderThoseGroups)
        {
            try
            {
                ObservableCollection<ItemWrapper> temp;
                temp = new ObservableCollection<ItemWrapper>(orderThoseGroups.OrderBy(p => p.Name));
                orderThoseGroups.Clear();
                foreach (ItemWrapper j in temp)
                {
                    orderThoseGroups.Add(j);
                }
            }
            catch (Exception)
            {
            }
            return orderThoseGroups;
        }

        private void GetAllItems()
        {
            try
            {
                Items.Clear();
                var items = _dataService.GetItems();
                foreach (var item in items)
                {
                    Items.Add(new ItemWrapper(item));
                }


            }
            catch (Exception)
            {

            }
        }

        private void Sort()
        {
            _items = ReorderItems(_items);
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

            Console.WriteLine(SelectedItem.Name);

            ProcessStartInfo processInfo;
            Process process;
            processInfo = new ProcessStartInfo("cmd.exe");
            processInfo.Arguments = "/c " + Quote + Settings.TeamViewerPath + Quote + " -i " + SelectedItem.ID + " -p " + SelectedItem.Password;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            process = Process.Start(processInfo);
            process.Close();

            if (Settings.CloseOnSubmit)
            {
                System.Windows.Application.Current.Shutdown();
            }
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

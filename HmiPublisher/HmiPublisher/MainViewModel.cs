using HmiPublisher.DataAccess;
using HmiPublisher.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Updater;

namespace HmiPublisher
{
    public enum Views
    {
        Edit = 1,
        Overview = 2
    }

    public class MainViewModel : Observable
    {

        public static MainViewModel Instance;

        const string Backslash = @"\";

        private RemoteWrapper _selectedItem;
        public RemoteWrapper SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
            }
        }

        private bool _inProgress;
        public bool InProgress
        {
            get { return _inProgress; }
            set
            {
                _inProgress = value;
                OnPropertyChanged(nameof(InProgress));
            }
        }

        private bool _saveEnabled = true;
        public bool SaveEnabled
        {
            get { return _saveEnabled; }
            set
            {
                _saveEnabled = value;
                OnPropertyChanged(nameof(SaveEnabled));
            }
        }

        private string _inProgressButtonText;
        public string InProgressButtonText
        {
            get { return _inProgressButtonText; }
            set
            {
                _inProgressButtonText = value;
                OnPropertyChanged(nameof(InProgressButtonText));
            }
        }

        private Views _currentView;
        public Views CurrentView
        {

            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private readonly IDataService _dataService;
        private readonly Publisher _publisher;

        public ObservableCollection<RemoteWrapper> Items { get; set; }
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand PublishCommand { get; private set; }
        public ICommand PublishAllCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand DeleteAllCommand { get; private set; }
        public ICommand ShowProgressCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }
        public ICommand GoToDestinationCommand { get; private set; }
        public ICommand RestartAppCommand { get; private set; }
        public ICommand RestartAllAppsCommand { get; private set; }
        public ICommand SwitchViewCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        public ModernWpf.ApplicationTheme? SystemTheme = ModernWpf.ApplicationTheme.Dark;

        public MainViewModel()
        {
            Instance = this;


            UpdaterInterface.Instance.Run(new UpdaterData
            {
                VersionFileUrl = "http://192.168.0.14:5551/_HmiPublisher_V3/version.txt",
                SourceFileUrl = "http://192.168.0.14:5551/_HmiPublisher_V3/HmiPublisher_SETUP.exe",
                TmpFilePath = Path.GetTempPath() + @"HmiPublisherV3\HmiPublisher_SETUP.exe",
                CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            });

            _dataService = new FileDataService();
            _publisher = new Publisher();

            Settings = new SettingsWrapper(_dataService.GetAppSettings());

            ChangeTheme();

            Items = new ObservableCollection<RemoteWrapper>();
            var items = _dataService.GetAll();
            foreach (var item in items)
            {
                Items.Add(new RemoteWrapper(item));
            }

            if (Items.Count() > 0)
            {
                SwitchCurrentPage(Views.Overview);
            }
            else
            {
                Items.Add(new RemoteWrapper(new Remote()
                {
                    Name = "PLC",
                    TargetUserName = "Administrator",
                    TargetPass = "Password",
                    SourcePath = @"src\hmi\OperatorPanel\bin\Debug\net48",
                    DestinationPath = @"\\192.168.2.178\hmi\",
                    ExecutableFilePath = @"C:\HMI\ProjectTemplate.exe",
                    Compression = Compression.Default,
                    Include = true,
                }));

                SwitchCurrentPage(Views.Edit);
            }

            foreach (var item in Items)
            {
                item.IpAddress = Utils.GetIpAddressFromString(item.DestinationPath);
                item.ProgressMessage = "---------";
            }


            AddCommand = new DelegateCommand((obj) =>
            {
                SwitchCurrentPage(Views.Edit);

                var item = new RemoteWrapper(new Remote() { Name = "---------" });
                Items.Add(item);
                SelectedItem = item;
            });
            DeleteCommand = new DelegateCommand((obj) =>
            {
                var selectedIndex = _selectedIndex;
                if (SelectedItem == null)
                {
                    return;
                }
                Items.Remove(SelectedItem);
                UpdateListSelection(Items.Count, selectedIndex);
            }, (arg) => !InProgress);
            CopyCommand = new DelegateCommand((obj) =>
            {
                SwitchCurrentPage(Views.Edit);

                var item = Utils.Clone(SelectedItem);
                item.ProgressMessage = "---------";
                item.IsError = false;
                item.ProgressPercentage = 0;
                item.IndeterminateProgressBar = false;
                Items.Add(item);
                SelectedItem = item;
            });
            SaveCommand = new DelegateCommand((obj) =>
            {
                var data = new List<Remote>();
                foreach (var item in Items)
                {
                    data.Add(item.Model);
                    SaveEnabled = false;
                    InvalidateCommands();
                }
                foreach (var item1 in Items)
                {
                    item1.IpAddress = Utils.GetIpAddressFromString(item1.DestinationPath);
                }
                try
                {
                    _dataService.SaveAll(data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, (arg) => true);
            PublishCommand = new DelegateCommand(async (obj) =>
            {
                SaveCommand.Execute(null);

                var x = Items.Where(a => a.InProgress && a.DestinationPath == SelectedItem.DestinationPath).Any();

                if (SelectedItem.InProgress || x)
                {
                    return;
                }

                SwitchCurrentPage(Views.Overview);

                if (!SanitizeEntries(SelectedItem, false))
                {
                    return;
                }

                InProgress = true;
                InvalidateCommands();

                await _publisher.PublishAsync(SelectedItem);

                InProgress = false;
                InvalidateCommands();

            }, (arg) => true);
            PublishAllCommand = new DelegateCommand(async (obj) =>
            {
                SaveCommand.Execute(null);

                if (InProgress)
                {
                    return;
                }

                SwitchCurrentPage(Views.Overview);

                var invalid = false;
                foreach (var item in Items)
                {
                    if (!SanitizeEntries(item, true))
                    {
                        invalid = true;
                    }
                }

                if (invalid)
                {
                    return;
                }

                InProgress = true;
                InvalidateCommands();

                var x = Items.Where(o => o.Include);
                await _publisher.PublishAllAync(x);

                InProgress = false;
                InvalidateCommands();

            }, (arg) => !InProgress);
            CancelCommand = new DelegateCommand((obj) =>
            {
                Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _publisher.Cancel();
                        //Thread.Sleep(300);
                        foreach (var item in Items)
                        {

                            item.ProgressMessage = "---------";
                            item.IsError = false;
                            item.IndeterminateProgressBar = false;
                            item.InProgress = false;
                            item.ProgressPercentage = 0;

                        }

                        InProgress = false;
                        InvalidateCommands();
                    });
                });
            }, (arg) => true);
            DeleteAllCommand = new DelegateCommand((obj) =>
            {
                var res = MessageBox.Show("Delete all?", Application.ResourceAssembly.GetName().Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes)
                {
                    return;
                }
                Items.Clear();
            }, (arg) => !InProgress);
            ShowProgressCommand = new DelegateCommand((_) =>
            {
                if (CurrentView == Views.Overview)
                {
                    SwitchCurrentPage(Views.Edit);
                }
                else
                {
                    SwitchCurrentPage(Views.Overview);
                }
            });
            OpenFolderCommand = new DelegateCommand((_) =>
            {
                try
                {
                    Process.Start("explorer.exe", _dataService.GetStorageFilePath());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            GoToDestinationCommand = new DelegateCommand((_) =>
            {
                try
                {
                    Process.Start("explorer.exe", SelectedItem.DestinationPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            RestartAppCommand = new DelegateCommand(async (_) =>
            {
                InProgress = true;
                InvalidateCommands();

                await _publisher.RestartApp(SelectedItem);

                InProgress = false;
                InvalidateCommands();

            }, (arg) => !InProgress);
            RestartAllAppsCommand = new DelegateCommand(async (_) =>
            {
                InProgress = true;
                InvalidateCommands();

                await _publisher.RestartAllApps(Items);

                InProgress = false;
                InvalidateCommands();

            }, (arg) => !InProgress);
            SwitchViewCommand = new DelegateCommand((_) =>
            {
                if (CurrentView == Views.Overview)
                {
                    SwitchCurrentPage(Views.Edit);
                }
                else
                {
                    SwitchCurrentPage(Views.Overview);
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

        public void SaveSettings()
        {
            _dataService.SaveAppSettings(Settings.Model);
            ChangeTheme();
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedItem))
            {

            }
        }

        private bool SanitizeEntries(RemoteWrapper item, bool isPublishAll)
        {
            if (!item.Include && isPublishAll)
            {
                return true;
            }

            if (_dataService.GetStorageFilePath().StartsWith(Backslash))
            {
                item.FullSourcePath = _dataService.GetStorageFilePath() + item.SourcePath;
            }
            else
            {
                item.FullSourcePath = _dataService.GetStorageFilePath() + Backslash + item.SourcePath;
            }

            if (item.FullSourcePath.Substring(item.FullSourcePath.Length - 1) != Backslash)
            {
                item.FullSourcePath = item.FullSourcePath + Backslash;
            }

            if (item.DestinationPath.Substring(item.DestinationPath.Length - 1) != Backslash)
            {
                item.DestinationPath = item.DestinationPath + Backslash;
            }

            if (!Path.HasExtension(item.ExecutableFilePath) && item.ExecutableFilePath.Length > 0 && item.ExecutableFilePath.Substring(item.ExecutableFilePath.Length - 1) == Backslash)
            {
                item.ExecutableFilePath = item.ExecutableFilePath.Remove(item.ExecutableFilePath.Length - 1, 1);
            }

            SaveCommand.Execute(null);

            if (item.Name == "")
            {
                item.ProgressMessage = "Name is required";
                item.IsError = true;
                return false;
            }
            else if (item.TargetUserName == "")
            {
                item.ProgressMessage = "User Name is required";
                item.IsError = true;
                return false;
            }
            else if (item.TargetPass == "")
            {
                item.ProgressMessage = "Password is required";
                item.IsError = true;
                return false;

            }
            else if (item.SourcePath.Length < 5 && Settings.BuildConfiguration == BuildConfiguration.None)
            {
                item.ProgressMessage = "Source is required";
                item.IsError = true;
                return false;
            }
            else if (!Directory.Exists(item.FullSourcePath) && Settings.BuildConfiguration == BuildConfiguration.None)
            {
                item.ProgressMessage = "Source path does not exist";
                item.IsError = true;
                return false;
            }
            else if (item.DestinationPath.Length < 5)
            {
                item.ProgressMessage = "Destination is required";
                item.IsError = true;
                return false;
            }
            else if (item.ExecutableFilePath.Length < 5)
            {
                item.ProgressMessage = "Executable path is required, should be C:\\hmi\\app or C:\\hmi\\app\\appname.exe";
                item.IsError = true;
                return false;
            }
            return true;
        }

        private void SwitchCurrentPage(Views page)
        {
            CurrentView = page;
            if (page == Views.Overview)
            {
                InProgressButtonText = "Edit";
            }
            else
            {
                InProgressButtonText = "Overview";
            }
        }



        private void UpdateListSelection(int count, int selectedIndex)
        {
            if (count == 0)
            {
                SelectedItem = null;
            }
            else if (count <= selectedIndex)
            {
                SelectedItem = Items[count - 1];
            }
            else
            {
                SelectedItem = Items[selectedIndex];
            }
        }


        public void InvalidateCommands()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((DelegateCommand)AddCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)CopyCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)PublishCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)PublishAllCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)CancelCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)DeleteAllCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)ShowProgressCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RestartAppCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RestartAllAppsCommand).RaiseCanExecuteChanged();
            });
        }
    }


}

using GitMonitor.DataAccess;
using GitMonitor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Updater;

namespace GitMonitor
{
    public class MainViewModel : Observable
    {
        public static MainViewModel Instance;
        const string Backslash = @"\";
        const string Quote = "\"";


        private AppSettingsWrapper _settings;
        public AppSettingsWrapper Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged(nameof(Settings));
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

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private string _currentDir;
        public string CurrentDir
        {
            get { return _currentDir; }
            set
            {
                _currentDir = value;
                OnPropertyChanged(nameof(CurrentDir));
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


        private readonly IDataService _dataService;

        public ObservableCollection<ItemWrapper> Items { get; set; }

        public ICommand CommitCommand { get; private set; }
        public ICommand CommitAllCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ShowDetailsCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }
        public ICommand AddToIgnoreListCommand { get; private set; }


        public string RootDir { get; set; }



        public MainViewModel()
        {
            Instance = this;


            UpdaterInterface.Instance.Run(new UpdaterData
            {
                VersionFileUrl = "http://192.168.0.14:5551/GitMonitor/version.txt",
                SourceFileUrl = "http://192.168.0.14:5551/GitMonitor/GitMonitor_SETUP.exe",
                TmpFilePath = Path.GetTempPath() + @"GitMonitor\GitMonitor_SETUP.exe",
                CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            });

            Items = new ObservableCollection<ItemWrapper>();

            _dataService = new FileDataService();

            Settings = new AppSettingsWrapper(_dataService.GetAppSettings());
            if (Settings.IgnoreList == null)
            {
                Settings.IgnoreList = new List<string>();
            }

            if (string.IsNullOrEmpty(Utils.GetCommandLineArgs()))
            {
                RootDir = Settings.DefaultFolder;
            }
            else
            {
                RootDir = Utils.GetCommandLineArgs();
            }

            ChangeTheme();



            var sillent = true;


            if (!string.IsNullOrEmpty(RootDir))
            {
                Scan();
            }


            CommitCommand = new DelegateCommand(async (obj) =>
            {
                InProgress = true;
                InvalidateCommands();
                await Task.Run(() =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Message = "Checking availability of remote server";
                        });



                        if (!SelectedItem.Include)
                        {
                            return;
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Message = "Pushing " + SelectedItem.Dir;
                        });

                        Utils.RunCommand(SelectedItem.Dir, "git add .", sillent);
                        Utils.RunCommand(SelectedItem.Dir, "git commit -m " + Quote + "Commit by GitMonitor" + Quote, sillent);
                        Utils.RunCommand(SelectedItem.Dir, "git push", sillent);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Message = "";
                            Items.Remove(SelectedItem);
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Message = ex.Message;
                        });
                    }
                });

                InProgress = false;
                InvalidateCommands();

            }, (obj) => !InProgress && SelectedItem != null);

            CommitAllCommand = new DelegateCommand(async (obj) =>
           {
               InProgress = true;
               InvalidateCommands();
               await Task.Run(() =>
                 {
                     try
                     {
                         Application.Current.Dispatcher.Invoke(() =>
                         {
                             Message = "Checking availability of remote server";
                         });

                         var items = new List<Item>();
                         foreach (var item in Items)
                         {
                             items.Add(item.Model);
                         }

                         foreach (var item in items)
                         {
                             if (!item.Include)
                             {
                                 return;
                             }

                             Application.Current.Dispatcher.Invoke(() =>
                             {
                                 Message = "Pushing " + item.Dir;
                             });

                             Utils.RunCommand(item.Dir, "git add .", sillent);
                             Utils.RunCommand(item.Dir, "git commit -m " + Quote + "Commit by GitMonitor" + Quote, sillent);
                             Utils.RunCommand(item.Dir, "git push", sillent);

                             var itemtodelete = Items.Where(x => x.Dir == item.Dir).FirstOrDefault();
                             Application.Current.Dispatcher.Invoke(() =>
                             {
                                 Items.Remove(itemtodelete);
                             });
                         }

                         Application.Current.Dispatcher.Invoke(() =>
                         {
                             Message = "";
                         });
                     }
                     catch (Exception ex)
                     {
                         Application.Current.Dispatcher.Invoke(() =>
                         {
                             Message = ex.Message;
                         });
                     }
                 });


               InProgress = false;
               InvalidateCommands();

           }, (obj) => !InProgress);

            RefreshCommand = new DelegateCommand((obj) =>
            {
                Scan();

            }, (obj) => !InProgress);

            ShowDetailsCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    Utils.RunCommand(SelectedItem.Dir, "git status && pause", false);
                }
                catch (Exception)
                {

                }

            }, (obj) => SelectedItem != null);

            SettingsCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    var d = new SettingsWindow(Settings);
                    d.ShowDialog();
                    Settings = Utils.Clone(d.DataContext as AppSettingsWrapper);
                    SaveSettings();
                }
                catch (Exception ex)
                {

                }
            }, (obj) => true);

            OpenFolderCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    Process.Start("explorer", SelectedItem.Dir);
                }
                catch (Exception ex)
                {

                }
            }, (obj) => SelectedItem != null);

            AddToIgnoreListCommand = new DelegateCommand((obj) =>
            {
                try
                {
                    if (!Settings.IgnoreList.Any(x => x == SelectedItem.Dir))
                    {
                        Settings.IgnoreList.Add(SelectedItem.Dir);
                        SaveSettings();
                        Items.Remove(SelectedItem);
                    }
                }
                catch (Exception ex)
                {

                }
            }, (obj) => SelectedItem != null);

        }

        public void SaveSettings()
        {
            _dataService.SaveAppSettings(Settings.Model);
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


        void Scan()
        {
            Message = "";
            Items.Clear();

            Task.Run(() =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InProgress = true;
                        InvalidateCommands();
                    });

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    TreeScan(RootDir);
                    stopwatch.Stop();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Items.Count == 0)
                        {
                            Message = "Everything up-to-date";
                        }
                        Message = Message + ", Elapsed time: " + stopwatch.ElapsedMilliseconds / 1000 + " sec";
                        InProgress = false;
                        InvalidateCommands();
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InProgress = false;
                        Message = ex.Message;
                        InvalidateCommands();
                    });
                }
            });
        }

        void TreeScan(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                if (d.Length > 250)
                {
                    continue;
                }

                CurrentDir = d;

                if (d.Contains("tc.net.extensibility"))
                {
                    continue;
                }

                if (d.EndsWith(".git"))
                {
                    int unpushedCount = 0;
                    int changesCount = 0;

                    var dir = d.Remove(d.IndexOf(".git"), 4);

                    if (Settings.IgnoreList.Any(x => x == dir))
                    {
                        continue;
                    }

                    var res = Utils.RunCommand(dir, "git cherry -v", true);
                    if (res.Contains("+"))
                    {
                        unpushedCount = res.Count(f => f == '+');
                    }

                    res = Utils.RunCommand(dir, "git diff --shortstat", true);
                    if (res != "")
                    {
                        res = Regex.Replace(res, @"\s+", "");
                        res = new String(res.TakeWhile(Char.IsDigit).ToArray());
                        changesCount = int.Parse(res);
                    }

                    if (changesCount == 0 && unpushedCount == 0)
                    {
                        continue;
                    }

                    if (changesCount > 0 || unpushedCount > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Items.Add(new ItemWrapper(new Item { Dir = dir, Changes = changesCount, UnpushedCommits = unpushedCount, Include = true }));
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Message = Items.Count + " Item(s) affected";
                            });
                        });
                    }
                }
                else
                {
                    TreeScan(d);
                }
            }
        }


        public void InvalidateCommands()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((DelegateCommand)CommitCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)CommitAllCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RefreshCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)ShowDetailsCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)OpenFolderCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)AddToIgnoreListCommand).RaiseCanExecuteChanged();
            });
        }

    }

    public class BoolToVisibility : IValueConverter
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

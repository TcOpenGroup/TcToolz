using BackupNow.DataAccess;
using BackupNow.Model;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Updater;

namespace BackupNow
{
    public class MainViewModel : Observable
    {
        public static MainViewModel Instance;
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

        //private string _message;
        //public string Message
        //{
        //    get { return _message; }
        //    set
        //    {
        //        _message = value;
        //        OnPropertyChanged(nameof(Message));
        //    }
        //}

        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                OnPropertyChanged(nameof(ProgressMessage));
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

        public ICommand RefreshCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }



        public MainViewModel()
        {
            Instance = this;

            UpdaterInterface.Instance.Run(new UpdaterData
            {
                VersionFileUrl = "http://192.168.0.14:5551/BackupNow/version.txt",
                SourceFileUrl = "http://192.168.0.14:5551/BackupNow/BackupNow_SETUP.exe",
                TmpFilePath = Path.GetTempPath() + @"BackupNow\BackupNow_SETUP.exe",
                CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            });

            if (!string.IsNullOrEmpty(Utils.GetCommandLineArgs()))
            {
                var d = new InitWindow();
                d.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            Items = new ObservableCollection<ItemWrapper>();

            _dataService = new FileDataService();

            Settings = new AppSettingsWrapper(_dataService.GetAppSettings());

            ChangeTheme();

            RefreshCommand = new DelegateCommand((obj) =>
            {
                Scan();

            }, (obj) => !InProgress);

            SettingsCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    var d = new SettingsWindow(Settings);
                    d.ShowDialog();
                    Settings = Utils.Clone(d.DataContext as AppSettingsWrapper);
                    SaveSettings();
                });
            }, (obj) => true);

            OpenFolderCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    Process.Start("explorer", SelectedItem.SourcePath);
                });

            }, (obj) => SelectedItem != null);

            CancelCommand = new DelegateCommand((obj) =>
            {
                Utils.RunWithErrorHandling(() =>
                {
                    _cancelSource.Cancel();
                });

            }, (obj) => InProgress);

            Scan();
        }

        public void SaveSettings()
        {
            Settings.BackupItems.RemoveAll(x => string.IsNullOrEmpty(x.Source) || string.IsNullOrEmpty(x.Destination));
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

        CancellationTokenSource _cancelSource;

        void Scan()
        {
            if (Settings.BackupItems.Count == 0)
            {
                SettingsCommand.Execute(null);
                if (Settings.BackupItems.Count == 0)
                {
                    return;
                }
            }

            _cancelSource = new CancellationTokenSource();

            _fileCount = 0;

            Items.Clear();
            ProgressMessage = "";
            InProgress = true;
            InvalidateCommands();

            Task.Run(() =>
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    foreach (var backupitem in Settings.BackupItems)
                    {
                        Application.Current.Dispatcher.Invoke(() => ProgressMessage = "Checking " + backupitem.Destination);
                        if (!backupitem.Enabled)
                        {
                            continue;
                        }
                        if (!System.IO.Directory.Exists(backupitem.Destination))
                        {
                            System.IO.Directory.CreateDirectory(backupitem.Destination);
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() => ProgressMessage = "Cleaning tmp files");
                    foreach (var backupitem in Settings.BackupItems)
                    {
                        if (!backupitem.Enabled)
                        {
                            continue;
                        }
                        foreach (string sFile in System.IO.Directory.GetFiles(backupitem.Destination, "*.tmp"))
                        {
                            System.IO.File.Delete(sFile);
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() => ProgressMessage = "");



                    foreach (var backupitem in Settings.BackupItems)
                    {
                        if (!backupitem.Enabled)
                        {
                            continue;
                        }

                        var existingZipFiles = new List<string>();
                        foreach (string f in Directory.EnumerateFiles(backupitem.Destination, "*.zip", SearchOption.TopDirectoryOnly))
                        {
                            existingZipFiles.Add(Path.GetFileNameWithoutExtension(f));
                        }

                        foreach (string f in Directory.EnumerateFiles(backupitem.Source, "*.backupnow", SearchOption.AllDirectories))
                        {
                            if (_cancelSource.IsCancellationRequested)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ProgressMessage = "";
                                    InProgress = false;
                                    InvalidateCommands();
                                });
                                return;
                            }

                            var zipFileName = Path.GetFileNameWithoutExtension(f);
                            var destZipFilePath = backupitem.Destination + @"\" + zipFileName + ".zip";

                            string[] lines = File.ReadAllLines(f);
                            var sourcePath = Path.GetDirectoryName(f);
                            var dirSize = DirSize(new DirectoryInfo(sourcePath));

                            var lastDirSize = lines.Length > 0 ? lines[0] : "";
                            var lastZipSize = lines.Length > 1 ? lines[1] : "";
                            var existingZipSize = "";
                            var isValidExistingZipFile = false;
                            if (File.Exists(destZipFilePath))
                            {
                                existingZipSize = new FileInfo(destZipFilePath).Length.ToString();
                                isValidExistingZipFile = IsValidZip(destZipFilePath);
                            }

                            if (lastDirSize == dirSize.ToString() && existingZipFiles.Any(x => x == zipFileName) && lastZipSize == existingZipSize && isValidExistingZipFile)
                            {
                                continue;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Items.Add(new ItemWrapper(new Item()
                                {
                                    FileName = zipFileName,
                                    SourcePath = sourcePath,
                                    SourceFilePath = f,
                                    DestinationPath = backupitem.Destination,
                                    NewSize = dirSize,
                                }));
                            });
                        }
                    }


                    foreach (var item in Items)
                    {
                        if (_cancelSource.IsCancellationRequested)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ProgressMessage = "";
                                InProgress = false;
                                InvalidateCommands();
                            });
                            return;
                        }
                        var rem = Items.Count - Items.IndexOf(item);

                        ProgressMessage = "Compressing " + item.FileName;
                        using (ZipFile zip = new ZipFile())
                        {
                            EventHandler<SaveProgressEventArgs> progressHandler = (o, i) =>
                            {
                                double copied = 100.0 / (double)i.EntriesTotal * (double)i.EntriesSaved;
                                int percentage = (int)copied;
                                if (percentage > 100)
                                {
                                    percentage = 100;
                                }
                                else if (percentage > 0)
                                {
                                    item.Progress = percentage;
                                }
                            };

                            zip.SaveProgress += progressHandler;
                            //zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                            zip.AddDirectory(item.SourcePath, item.FileName);
                            zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                            zip.Save(item.DestinationPath + @"\" + item.FileName + ".zip");
                        }

                        var newZipFileLength = new FileInfo(item.DestinationPath + @"\" + item.FileName + ".zip").Length;

                        File.WriteAllLines(item.SourceFilePath, new string[] { item.NewSize.ToString(), newZipFileLength.ToString() });

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Progress = 100;
                        });
                    }

                    stopwatch.Stop();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressMessage = "Everything up-to-date" + ", passed objects: " + _fileCount.ToString() + ", elapsed time: " + stopwatch.ElapsedMilliseconds / 1000 + " sec";
                        InProgress = false;
                        InvalidateCommands();
                    });

                    if (Settings.ShutdownWhenFinished)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShuttingDownWindow shuttingDownWindow = new ShuttingDownWindow();
                            shuttingDownWindow.ShowDialog();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InProgress = false;
                        ProgressMessage = ex.Message;
                        InvalidateCommands();
                    });
                }
            }, _cancelSource.Token);
        }

        int _fileCount = 0;
        public long DirSize(DirectoryInfo d)
        {
            long size = 0;

            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                ProgressMessage = "Scanning " + fi.FullName;

                if (fi.Extension != ".backupnow")
                {
                    _fileCount++;
                    size += fi.Length;
                }
                //Thread.Sleep(1);
            }

            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        bool IsValidZip(string file)
        {
            var res = false;
            try
            {
                using (ZipFile zip = new ZipFile(file))
                {
                    res = true;
                }
            }
            catch (Exception)
            {
            }

            return res;
        }

        public void InvalidateCommands()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((DelegateCommand)RefreshCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)OpenFolderCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)CancelCommand).RaiseCanExecuteChanged();
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

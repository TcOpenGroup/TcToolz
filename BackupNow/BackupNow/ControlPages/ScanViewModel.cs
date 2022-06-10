using Autofac;
using BackupNow.Model;
using Ionic.Zip;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BackupNow
{
    public class ScanViewModel : Observable
    {
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

        private string _message1;
        public string Message1
        {
            get { return _message1; }
            set
            {
                _message1 = value;
                OnPropertyChanged(nameof(Message1));
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

        private MainViewModel _mainViewModel;

        public ObservableCollection<ItemWrapper> Items { get; set; }

        public ICommand RefreshCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public ScanViewModel()
        {
            _mainViewModel = Bootstrapper.Container.Resolve<MainViewModel>();

            Items = new ObservableCollection<ItemWrapper>();

            RefreshCommand = new DelegateCommand((obj) =>
            {
                Scan();

            }, (obj) => !InProgress);

            OpenFolderCommand = new DelegateCommand((obj) =>
            {
                if (SelectedItem == null)
                {
                    return;
                }
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

            Bootstrapper.Container.Resolve<IEventAggregator>().GetEvent<KeyPressed>().Subscribe((value) =>
            {
                if (value == KeyAction.Refresh)
                {
                    Scan();
                }
                else if (value == KeyAction.ShowDetails)
                {
                    OpenFolderCommand.Execute(null);
                }
                else if (value == KeyAction.Esc)
                {
                    if (InProgress)
                    {
                        CancelCommand.Execute(null);
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
            });
        }


        CancellationTokenSource _cancelSource;

        void Scan()
        {
            if (_mainViewModel.Settings.BackupItems.Count == 0)
            {
                Message1 = "Nothing to sync";
                return;
            }

            _cancelSource = new CancellationTokenSource();

            _fileCount = 0;

            Items.Clear();
            InProgress = true;
            InvalidateCommands();

            var settings = Utils.Clone(_mainViewModel.Settings);

            Task.Run(() =>
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    foreach (var backupitem in settings.BackupItems)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Message1 = "Checking destinations ...";
                            ProgressMessage = backupitem.Destination;
                        });
                        if (!backupitem.Enabled)
                        {
                            continue;
                        }
                        if (!System.IO.Directory.Exists(backupitem.Destination))
                        {
                            System.IO.Directory.CreateDirectory(backupitem.Destination);
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Cleaning tmp files ...";
                    });
                    foreach (var backupitem in settings.BackupItems)
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

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Scanning ...";
                    });

                    var a = new List<ItemToDelete>();
                    var b = new List<string>();

                    foreach (var backupitem in settings.BackupItems)
                    {
                        if (!backupitem.Enabled)
                        {
                            continue;
                        }

                        var existingZipFiles = new List<string>();
                        foreach (string f in Directory.EnumerateFiles(backupitem.Destination, "*.zip", SearchOption.TopDirectoryOnly))
                        {
                            existingZipFiles.Add(Path.GetFileNameWithoutExtension(f));
                            if (!a.Exists(x => x.FileName == Path.GetFileNameWithoutExtension(f)))
                            {
                                a.Add(new ItemToDelete { FileName = Path.GetFileNameWithoutExtension(f), FullPath = f });
                            }
                        }

                        foreach (string f in Directory.EnumerateFiles(backupitem.Source, "*.backupnow", SearchOption.AllDirectories))
                        {
                            if (_cancelSource.IsCancellationRequested)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Message1 = "Cancelled";
                                    InProgress = false;
                                    InvalidateCommands();
                                });
                                return;
                            }

                            var zipFileName = Path.GetFileNameWithoutExtension(f);
                            b.Add(zipFileName);

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


                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Cleaning untracked zip files ...";
                    });

                    foreach (var item in a)
                    {
                        if (!b.Any(x => x == item.FileName))
                        {
                            try
                            {
                                File.Delete(item.FullPath);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }



                    foreach (var item in Items)
                    {
                        if (_cancelSource.IsCancellationRequested)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Message1 = "Cancelled by user";
                                InProgress = false;
                                InvalidateCommands();
                            });
                            return;
                        }
                        var rem = Items.Count - Items.IndexOf(item);

                        ProgressMessage = item.FileName;
                        Message1 = "Compressing ...";
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
                        Message1 = "Everything up-to-date";
                        ProgressMessage = "Passed objects: " + _fileCount.ToString() + ", elapsed time: " + stopwatch.ElapsedMilliseconds / 1000 + " sec";
                        InProgress = false;
                        InvalidateCommands();
                    });

                    if (settings.ShutdownWhenFinished)
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
                        Message1 = "Failed";
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
                ProgressMessage = fi.FullName;

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
    public class ItemToDelete
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
    }
}

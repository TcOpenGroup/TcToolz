﻿using Autofac;
using BackupNow.Model;
using Ionic.Zip;
using Newtonsoft.Json;
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


            Collection<Item> SavedItems = null;
            Collection<Item> ProjectItems = new Collection<Item>(); ;

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
                        if (!Directory.Exists(backupitem.Destination))
                        {
                            Directory.CreateDirectory(backupitem.Destination);
                        }
                    }

                    //delete incomplete tmp (ZIP) files
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
                        foreach (string sFile in Directory.GetFiles(backupitem.Destination, "*.tmp"))
                        {
                            File.Delete(sFile);
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Scanning ...";
                    });

                    var a = new List<ItemToDelete>();
                    //var b = new List<string>();

                    //check file exists
                    var savedItemsFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow\ProjectItems.json";
                    if (File.Exists(savedItemsFile))
                    {
                        var jsonItems = File.ReadAllText(savedItemsFile);
                        SavedItems = JsonConvert.DeserializeObject<Collection<Item>>(jsonItems);
                    }

                    foreach (var backupitem in settings.BackupItems)
                    {
                        if (!backupitem.Enabled)
                        {
                            continue;
                        }

                        //TODO: to delete?
                        var existingZipFiles = new List<string>();
                        foreach (string f in Directory.EnumerateFiles(backupitem.Destination, "*.zip", SearchOption.TopDirectoryOnly))
                        {
                            existingZipFiles.Add(Path.GetFileNameWithoutExtension(f));
                            if (!a.Exists(x => x.FileName == Path.GetFileNameWithoutExtension(f)))
                            {
                                a.Add(new ItemToDelete { FileName = Path.GetFileNameWithoutExtension(f), FullPath = f });
                            }
                        }

                        if (backupitem.IsYearFolder)
                        {
                            foreach (string yearFolder in Directory.GetDirectories(backupitem.Source, "20*")) //TODO: yearPattern as parameter
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

                                var year = Path.GetFileName(yearFolder);

                                Directory.CreateDirectory(backupitem.Destination + year);

                                foreach (string project in Directory.GetDirectories(yearFolder))
                                {
                                    //var f = project + @"\info.backupnow";
                                    var zipFileName = Path.GetFileName(project);  //Path.GetFileNameWithoutExtension(f);
                                                                                  //b.Add(zipFileName);

                                    var destZipFilePath = backupitem.Destination + year + @"\" + zipFileName +".zip";

                                    //string[] lines = File.ReadAllLines(f);
                                    //var sourcePath = Path.GetDirectoryName(f);
                                    var dirChange = DirTicks(new DirectoryInfo(project));

                                    //var lastDirChange = lines.Length > 0 ? lines[0] : "";
                                    //var lastZipSize = lines.Length > 1 ? lines[1] : "";
                                    var existingZipSize = "";
                                    var isValidExistingZipFile = false;
                                    if (File.Exists(destZipFilePath))
                                    {
                                        existingZipSize = new FileInfo(destZipFilePath).Length.ToString();
                                        isValidExistingZipFile = IsValidZip(destZipFilePath);
                                    }

                                    //add actual project to ProjectItems collection
                                    ProjectItems.Add(new Item()
                                    {
                                        FileName = Path.GetFileName(project),
                                        SourcePath = project,
                                        DestinationPath = destZipFilePath, //backupitem.Destination,
                                        NewChange = dirChange
                                    });


                                    //if (lastDirChange == dirChange.ToString() && existingZipFiles.Any(x => x == zipFileName) && lastZipSize == existingZipSize && isValidExistingZipFile)
                                    //{
                                    //    continue;
                                    //}

                                    //check if file exists in saved items
                                    if (SavedItems != null)
                                    {
                                        var savedItem = SavedItems.FirstOrDefault(x => x.FileName == zipFileName);
                                        if (savedItem != null)
                                        {
                                            if (savedItem.NewChange == dirChange)
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        Items.Add(new ItemWrapper(new Item()
                                        {
                                            FileName = Path.GetFileName(project),
                                            SourcePath = project,
                                            //SourceFilePath = project + @"\info.backupnow",
                                            DestinationPath = destZipFilePath, //backupitem.Destination,
                                            NewChange = dirChange
                                        }));
                                    });
                                }
                            }
                        }
                        else
                        {
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
                                var destZipFilePath = Path.Combine(backupitem.Destination, zipFileName + ".zip"); 
                                //destZipFilePath = backupitem.Destination + @"\" + zipFileName + ".zip";

                                //string[] lines = File.ReadAllLines(f);
                                var sourcePath = Path.GetDirectoryName(f);
                                var dirChange = DirTicks(new DirectoryInfo(sourcePath));

                                //var lastDirChange = lines.Length > 0 ? lines[0] : "";
                                //var lastZipSize = lines.Length > 1 ? lines[1] : "";
                                var existingZipSize = "";
                                var isValidExistingZipFile = false;
                                if (File.Exists(destZipFilePath))
                                {
                                    existingZipSize = new FileInfo(destZipFilePath).Length.ToString();
                                    isValidExistingZipFile = IsValidZip(destZipFilePath);
                                }

                                //add actual project to ProjectItems collection
                                ProjectItems.Add(new Item()
                                {
                                    FileName = zipFileName,
                                    SourcePath = sourcePath,
                                    DestinationPath = destZipFilePath, //backupitem.Destination,
                                    NewChange = dirChange
                                });


                                //if (lastDirChange == dirChange.ToString() && existingZipFiles.Any(x => x == zipFileName) && lastZipSize == existingZipSize && isValidExistingZipFile)
                                //{
                                //    continue;
                                //}

                                //check if file exists in saved items
                                if (SavedItems != null)
                                {
                                    var savedItem = SavedItems.FirstOrDefault(x => x.FileName == zipFileName);
                                    if (savedItem != null)
                                    {
                                        if (savedItem.NewChange == dirChange)
                                        {
                                            continue;
                                        }
                                    }
                                }

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Items.Add(new ItemWrapper(new Item()
                                    {
                                        FileName = zipFileName,
                                        SourcePath = sourcePath,
                                        //SourceFilePath = f + @"\info.backupnow",
                                        DestinationPath = destZipFilePath, //backupitem.Destination,
                                        NewChange = dirChange
                                    }));
                                });
                            }
                            
                        }
                    }


                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    Message1 = "Cleaning untracked zip files ...";
                    //});

                    //foreach (var item in a)
                    //{
                    //    if (!b.Any(x => x == item.FileName))
                    //    {
                    //        try
                    //        {
                    //            File.Delete(item.FullPath);
                    //        }
                    //        catch (Exception)
                    //        {
                    //        }
                    //    }
                    //}


                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Copying previous backups ...";
                    });
                    foreach (var item in Items)
                    {
                        ProgressMessage = item.FileName;

                        var fpath = item.DestinationPath; // + @"\" + item.FileName + ".zip";
                        if (File.Exists(fpath))
                        {
                            if (!Directory.Exists(Path.GetDirectoryName(fpath) + @"\.prev\"))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(fpath) + @"\.prev\");
                            }
                            File.Copy(fpath, Path.GetDirectoryName(fpath) + @"\.prev\" + item.FileName + ".zip", true);
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
                        //TODO: to delete?
                        var rem = Items.Count - Items.IndexOf(item);

                        Message1 = "Compressing ...";
                        ProgressMessage = item.FileName;
                        var zipFilePath = "";
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
                            zipFilePath = item.DestinationPath; // + @"\" + item.FileName + ".zip";
                            zip.Save(zipFilePath);
                        }


                        var newZipFileLength = new FileInfo(zipFilePath).Length;

                        //File.WriteAllLines(item.SourceFilePath, new string[] { item.NewChange.ToString(), newZipFileLength.ToString() });

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Progress = 100;
                        });
                    }


                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Verifying backups ...";
                    });
                    foreach (var item in Items)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProgressMessage = item.FileName;
                        });
                        var fpath = item.DestinationPath; // + @"\" + item.FileName + ".zip";
                        if (!File.Exists(fpath) || !IsValidZip(fpath))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Message1 = "Backup Failed";
                                ProgressMessage = item.FileName + " is not valid!";
                                InProgress = false;
                                InvalidateCommands();
                            });
                            return;
                        }
                    }

                    //Save Items to json file
                    var json = JsonConvert.SerializeObject(ProjectItems);
                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow\ProjectItems.json", json);

                    stopwatch.Stop();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Message1 = "Everything up-to-date";
                        ProgressMessage = "Passed objects: " + _fileCount.ToString() + ", elapsed time: " + stopwatch.ElapsedMilliseconds / 1000 + " sec";
                        InProgress = false;
                        InvalidateCommands();
                    });

                    if (_mainViewModel.Settings.ShutdownWhenFinished)
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
            }

            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public long DirTicks(DirectoryInfo directory)
        {
            //get file count from directory
            _fileCount += Directory.GetFiles(directory.FullName, "*", SearchOption.AllDirectories).Length;

            //ROOT
            DateTime latestDirChange = directory.LastWriteTimeUtc;

            //FILE SYSTEM ENTRIES
            string[] fileSystemEntries = Directory.GetFileSystemEntries(directory.FullName, "*", SearchOption.AllDirectories);
            DateTime latestFileSystemChange = fileSystemEntries.Select(f => File.GetLastWriteTimeUtc(f)).OrderByDescending(t => t).FirstOrDefault();

            //COMPARE
            DateTime maxDt = (DateTime.Compare(latestDirChange, latestFileSystemChange) > 0) ? latestDirChange : latestFileSystemChange;

            return maxDt.Ticks;
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

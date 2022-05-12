using HmiPublisher.Model;
using HmiPublisher.UI.Command;
using HmiPublisher.UI.DataProvider;
using HmiPublisher.UI.DataProvider.Lookups;
using HmiPublisher.UI.Events;
using HmiPublisher.UI.View.Services;
using HmiPublisher.UI.Wrapper;
using Libs.Network;
using Libs.Other;
using Libs.Packaging;
using Libs.Utils;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HmiPublisher.UI.ViewModel
{


    public interface IRemoteEditViewModel
    {
        void Load(int? remoteId = null, NavigationItemViewModel item = null);
        void Publish();
        void Abort();
        void Save();
        RemoteWrapper Remote { get; }
    }



    public class RemoteEditViewModel : Observable, IRemoteEditViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IRemoteDataProvider _remoteDataProvider;
        private RemoteWrapper _remote;
        private IEnumerable<LookupItem> _remoteGroups;

        public RemoteEditViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            INavigationViewModel navigationViewModel,
            IRemoteDataProvider remoteDataProvider)
        {
            _eventAggregator = eventAggregator;
            _messageDialogService = messageDialogService;
            _remoteDataProvider = remoteDataProvider;
            NavigationViewModel = navigationViewModel;

            SaveCommand = new DelegateCommand(OnSaveExecute, OnSaveCanExecute);
            ResetCommand = new DelegateCommand(OnResetExecute, OnResetCanExecute);
            DeleteCommand = new DelegateCommand(OnDeleteExecute, OnDeleteCanExecute);
            PublishCommand = new DelegateCommand(OnPublishExecute, OnPublishCommandCanExecute);
            RestartAppCommand = new DelegateCommand(OnRestartAppExecute, OnRestartAppCommandCanExecute);


        }
        public INavigationViewModel NavigationViewModel { get; private set; }

        public NavigationItemViewModel NavigationItemViewModel { get; private set; }
        public void Load(int? remoteId = null, NavigationItemViewModel item = null)
        {
            NavigationItemViewModel = item;
            var remote = remoteId.HasValue
                ? _remoteDataProvider.GetRemoteById(remoteId.Value)
                : new Remote { };

            Remote = new RemoteWrapper(remote);
            Remote.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Remote.IsChanged)
                || e.PropertyName == nameof(Remote.IsValid))
                {
                    InvalidateCommands();
                }
            };

            NavigationViewModel.Load();

            InvalidateCommands();
        }

        public RemoteWrapper Remote
        {
            get => _remote;
            private set
            {
                _remote = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LookupItem> RemoteGroupLookup
        {
            get => _remoteGroups;
            set
            {
                _remoteGroups = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; private set; }

        public ICommand ResetCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand PublishCommand { get; private set; }
        public ICommand RestartAppCommand { get; private set; }



        private void OnSaveExecute(object obj)
        {
            _remoteDataProvider.SaveRemote(Remote.Model);
            Remote.AcceptChanges();
            _eventAggregator.GetEvent<RemoteSavedEvent>().Publish(Remote.Model);


            var nav = NavigationViewModel as NavigationViewModel;
            foreach (var item in nav.NavigationItems)
            {
                if (item.RemoteId == Remote.Id)
                {
                    NavigationItemViewModel = item;
                }
            }

            InvalidateCommands();
        }

        private bool OnSaveCanExecute(object arg)
        {
            return Remote.IsChanged && Remote.IsValid;
        }

        private void OnResetExecute(object obj)
        {
            Remote.RejectChanges();
        }

        private bool OnResetCanExecute(object arg)
        {
            return Remote.IsChanged;
        }

        private bool OnDeleteCanExecute(object arg)
        {
            return Remote != null && Remote.Id > 0;
        }
        private bool OnPublishCommandCanExecute(object arg)
        {
            return !Remote.InProgress;
        }
        private bool OnRestartAppCommandCanExecute(object arg)
        {
            return !Remote.InProgress;
        }
        private void OnDeleteExecute(object obj)
        {
            var result = MessageBox.Show("Do you really want to delete the remote", Remote.Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _remoteDataProvider.DeleteRemote(Remote.Id);
                _eventAggregator.GetEvent<RemoteDeletedEvent>().Publish(Remote.Id);
            }
        }
        private void OnPublishExecute(object obj)
        {
            Save();

            Remote.ProgressMessage = "Cleaning local folder...";
            foreach (string file in Directory.GetFiles(Remote.SourcePath, "*.tmp").Where(x => x.EndsWith(".tmp")))
            {
                File.Delete(file);
            }

            Directory.CreateDirectory(Remote.SourcePath);

            if (File.Exists(Remote.SourcePath + @"\" + "app.zip"))
            {
                File.Delete(Remote.SourcePath + @"\" + "app.zip");
            }

            Publish();
        }

        private void OnRestartAppExecute(object obj)
        {
            if (!XIPAddress.CorrectIP(XIPAddress.GetIpAddressFromString(Remote.DestinationPath)))
            {
                return;
            }

            _targetIpAddr = XIPAddress.GetIpAddressFromString(Remote.DestinationPath);

            var pub = new HmiPublisherServer();
            var appProcessName = Remote.ExecutableFilePath.Substring(Remote.ExecutableFilePath.LastIndexOf(@"\") + 1);

            Task.Factory.StartNew(() =>
            {
                Sync.CommandThread.Add(new ThreadWrapper() { Id = Remote.Id.ToString(), Thread = Thread.CurrentThread });

                Remote.InProgress = true;
                NavigationItemViewModel.IsIndeterminate = true;
                InvalidateCommands();

                var command = @"system; taskkill /f /im " + appProcessName;
                var res = pub.ConnectAndSend(_targetIpAddr, command);

                command = @"system; start " + Remote.ExecutableFilePath;
                res = pub.ConnectAndSend(_targetIpAddr, command);

                Remote.InProgress = false;
                NavigationItemViewModel.IsIndeterminate = false;
                InvalidateCommands();

            }, TaskCreationOptions.LongRunning);

        }


        private void InvalidateCommands()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)ResetCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)PublishCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RestartAppCommand).RaiseCanExecuteChanged();
            });
        }

        private long _totalFileLenghtBytes { get; set; }
        private long _copiedFileLenghtBytes { get; set; }
        private long _totalFileLenght { get; set; }
        private long _copiedFileLenght { get; set; }
        private double _tmpProgressPercentage { get; set; }
        private long _copiedFileLenghtMbytes { get; set; }
        private long _totalFileLenghtMbytes { get; set; }
        string _targetIpAddr { get; set; }
        string _appProcessName { get; set; }
        bool _compressed { get; set; }
        bool _cancelled { get; set; }
        DateTime _lastUpdate { get; set; }



        public void Publish()
        {
            Abort();

            Remote.ProgressMessage = "Initializing ... ";
            _cancelled = false;
            _copiedFileLenghtBytes = 0;
            _copiedFileLenght = 0;
            _totalFileLenghtBytes = 0;
            _totalFileLenght = 0;

            Remote.IsError = false;
            Remote.IsSuccess = false;
            Remote.ProgressPercentage = 0;
            Remote.InProgress = true;
            NavigationItemViewModel.ProgressPercentage = 0;
            NavigationItemViewModel.IsIndeterminate = true;

            InvalidateCommands();

            try
            {

                if (Remote.DestinationPath.Substring(Remote.DestinationPath.Length - 1, 1) != @"\")
                {
                    Remote.DestinationPath = Remote.DestinationPath + @"\";
                }
                if (XIPAddress.CorrectIP(XIPAddress.GetIpAddressFromString(Remote.DestinationPath)))
                {
                    _targetIpAddr = XIPAddress.GetIpAddressFromString(Remote.DestinationPath);
                }
                if (!Directory.Exists(Remote.SourcePath))
                {
                    Error(Remote, "Output path does not exists");
                }
                else if (!XIPAddress.PingHost(_targetIpAddr))
                {
                    Error(Remote, "Connection failed");
                }
                else
                {
                    _appProcessName = Remote.ExecutableFilePath.Substring(Remote.ExecutableFilePath.LastIndexOf(@"\") + 1);

                    //ThreadStart action = () =>
                    //{
                    //};
                    //Thread thread = new Thread(action) { IsBackground = true };
                    //thread.Start();


                    Task.Factory.StartNew(() =>
                    {


                        Sync.MainThreads.Add(new ThreadWrapper() { Id = Remote.Id.ToString(), Thread = Thread.CurrentThread });


                        if (Remote.SourcePath.Substring(Remote.SourcePath.Length - 1) != @"\")
                        {
                            Remote.SourcePath = Remote.SourcePath + @"\";
                        }

                        var pub = new HmiPublisherServer();

                        var command = "";

                        var preparationErrorOccured = false;
                        var preparationDone = false;

                        Task.Factory.StartNew(() =>
                        {
                            Sync.PrepareThread.Add(new ThreadWrapper() { Id = Remote.Id.ToString(), Thread = Thread.CurrentThread });
                            //Remote.ProgressMessage = "Initializing remote machine ...";
                            command = @"system; taskkill /f /im HmiPublisherServer.exe";
                            var res = pub.ConnectAndSend(_targetIpAddr, command);
                            if (!res)
                            {
                                preparationErrorOccured = true;
                                return;
                            }

                            command = @"system; taskkill /f /im " + _appProcessName;
                            res = pub.ConnectAndSend(_targetIpAddr, command);
                            if (!res)
                            {
                                preparationErrorOccured = true;
                                return;
                            }

                            //Remote.ProgressMessage = "Cleaning shared folder ...";
                            command = @"system; del /q /s " + "\"" + Remote.ExecutableFilePath.Substring(0, Remote.ExecutableFilePath.LastIndexOf(@"\") + 1) + "\"";
                            pub.ConnectAndSend(_targetIpAddr, command);


                            Cmd.RunCommand(@"net use \\" + _targetIpAddr + " /user:" + Remote.TargetUserName + " " + Remote.TargetPass);


                            //Remote.ProgressMessage = "Waiting ...";
                            if (!SendHmiPublisherServer(Remote))
                            {
                                preparationErrorOccured = true;
                                return;
                            }

                            preparationDone = true;

                        }, TaskCreationOptions.LongRunning);


                        Remote.ProgressMessage = "Waiting for another task ...";
                        if (Sync.SpinLock.WaitOne())
                        {
                            if (!File.Exists(Remote.SourcePath + "app.zip"))
                            {
                                try
                                {
                                    Remote.ProgressMessage = "Compressing ...";
                                    Package.CreateZipFile(Remote.SourcePath, Remote.SourcePath + "app.zip", Remote.Compression);
                                }
                                catch (Exception)
                                {
                                    Sync.SpinLock.ReleaseMutex();
                                }
                                finally
                                {
                                    try
                                    {
                                        Sync.SpinLock.ReleaseMutex();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }

                        Remote.ProgressMessage = "Initializing remote machine ...";
                        while (!preparationDone)
                        {
                            if (preparationErrorOccured)
                            {
                                Error(Remote, "Cannot get access to the shared folder");
                                return;
                            }
                        }

                        _totalFileLenghtBytes = FileSystem.GetFileLenght(Remote.SourcePath + "app.zip");
                        command =
                       @"system; start " + Remote.ExecutableFilePath.Substring(0, Remote.ExecutableFilePath.LastIndexOf(@"\") + 1) +
                       "HmiPublisherServer.exe" +
                       " " +
                       _totalFileLenghtBytes.ToString() +
                       " " +
                       "\"" + Remote.ExecutableFilePath + "\"" +
                       " " +
                       "\"" + _appProcessName + "\"";
                        pub.ConnectAndSend(_targetIpAddr, command);


                        Task.Factory.StartNew(() =>
                        {
                            Sync.MeasureThread.Add(new ThreadWrapper() { Id = Remote.Id.ToString(), Thread = Thread.CurrentThread });
                            MeasureProgressAsyncTask(Remote);
                        }, TaskCreationOptions.LongRunning);

                        Task.Factory.StartNew(() =>
                        {
                            Sync.CopyThread.Add(new ThreadWrapper() { Id = Remote.Id.ToString(), Thread = Thread.CurrentThread });
                            CopyFile(Remote.SourcePath + @"\" + "app.zip", Remote.DestinationPath + "app.zip");
                        }, TaskCreationOptions.LongRunning);
                    }, TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception ex)
            {
                Error(Remote, ex.Message);
                Sync.SpinLock.ReleaseMutex();
            }
        }

        public void CopyFile(string sourcePath, string destinationPath)
        {
            using (Stream source = File.Open(sourcePath, FileMode.Open))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    source.CopyTo(destination);
                }
            }
        }

        private void MeasureProgressAsyncTask(RemoteWrapper remote)
        {
            while (!_cancelled)
            {
                try
                {
                    _copiedFileLenghtBytes = FileSystem.GetFileLenght(remote.DestinationPath + @"app.zip");
                    _tmpProgressPercentage = 100.0 / _totalFileLenghtBytes * _copiedFileLenghtBytes;
                    _copiedFileLenghtMbytes = _copiedFileLenghtBytes / 1000000;
                    _totalFileLenghtMbytes = _totalFileLenghtBytes / 1000000;
                    remote.ProgressPercentage = (int)_tmpProgressPercentage;
                    NavigationItemViewModel.ProgressPercentage = (int)_tmpProgressPercentage;
                    NavigationItemViewModel.IsIndeterminate = false;
                    remote.IndeterminateProgressBar = false;

                    remote.ProgressMessage = "Uploading " + remote.ProgressPercentage.ToString() + "% (" + _copiedFileLenghtMbytes.ToString() + " MB of " + _totalFileLenghtMbytes.ToString() + " MB)";

                    if (_copiedFileLenghtBytes >= _totalFileLenghtBytes)
                    {

                        remote.IsSuccess = true;
                        remote.ProgressPercentage = 100;
                        NavigationItemViewModel.ProgressPercentage = 100;
                        remote.ProgressMessage = "Done";
                        remote.IndeterminateProgressBar = false;
                        remote.InProgress = false;

                        InvalidateCommands();


                        _cancelled = true;
                        break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Error(RemoteWrapper remote, string message)
        {
            remote.ProgressMessage = message;
            remote.IsError = true;
            remote.InProgress = false;
            NavigationItemViewModel.IsIndeterminate = false;

            InvalidateCommands();

        }

        private bool SendHmiPublisherServer(RemoteWrapper remote)
        {
            if (XFile.Copy("HmiPublisherServer.exe", remote.DestinationPath + "HmiPublisherServer.exe") && XFile.Copy("DotNetZip.dll", remote.DestinationPath + "DotNetZip.dll"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public void Abort()
        {
            _cancelled = true;

            Remote.InProgress = false;
            Remote.ProgressMessage = "Aborted";
            Remote.ProgressPercentage = 0;

            InvalidateCommands();



            if (NavigationItemViewModel != null)
            {
                NavigationItemViewModel.IsIndeterminate = false;
                NavigationItemViewModel.ProgressPercentage = 0;
            }
        }

        public void Save()
        {
            OnSaveExecute(this);
        }
    }
}

using HmiPublisher.UI.Command;
using HmiPublisher.UI.DataProvider;
using HmiPublisher.UI.Events;
using HmiPublisher.UI.View.Services;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace HmiPublisher.UI.ViewModel
{
    public class MainViewModel : Observable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMessageDialogService _messageDialogService;
        private IRemoteEditViewModel _selectedRemoteEditViewModel;
        private readonly IRemoteDataProvider _remoteDataProvider;

        private Func<IRemoteEditViewModel> _remoteEditViewModelCreator;


        public MainViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IRemoteDataProvider remoteDataProvider,
            INavigationViewModel navigationViewModel,
            Func<IRemoteEditViewModel> remoteEditViewModelCreator)
        {
            _eventAggregator = eventAggregator;
            _messageDialogService = messageDialogService;
            _remoteDataProvider = remoteDataProvider;
            _eventAggregator.GetEvent<OpenRemoteEditViewEvent>().Subscribe(OnOpenRemoteTab);
            _eventAggregator.GetEvent<RemoteDeletedEvent>().Subscribe(OnRemoteDeleted);

            NavigationViewModel = navigationViewModel;
            _remoteEditViewModelCreator = remoteEditViewModelCreator;
            RemoteEditViewModels = new ObservableCollection<IRemoteEditViewModel>();
            CloseRemoteTabCommand = new DelegateCommand(OnCloseRemoteTabExecute);
            AddRemoteCommand = new DelegateCommand(OnAddRemoteExecute);

            PublishAllCommand = new DelegateCommand(OnPublishAllExecute);
            CancelAllCommand = new DelegateCommand(OnCancelAllExecute);
            DeleteAllCommand = new DelegateCommand(OnDeleteAllExecute);
            SaveAllCommand = new DelegateCommand(OnSaveAllExecute);
        }

        public void Load()
        {
            NavigationViewModel.Load();
        }

        public void OnClosing(CancelEventArgs e)
        {
            if (RemoteEditViewModels.Any(f => f.Remote.IsChanged))
            {
                var result = System.Windows.MessageBox.Show("You'll lose your changes if you close this application. Close it?", "Close application?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                e.Cancel = result != System.Windows.MessageBoxResult.Yes;
            }
        }

        public ICommand CloseRemoteTabCommand { get; private set; }
        public ICommand AddRemoteCommand { get; set; }
        public ICommand PublishAllCommand { get; set; }
        public ICommand CancelAllCommand { get; set; }
        public ICommand DeleteAllCommand { get; set; }
        public ICommand SaveAllCommand { get; set; }

        public INavigationViewModel NavigationViewModel { get; private set; }

        public ObservableCollection<IRemoteEditViewModel> RemoteEditViewModels { get; private set; }

        public IRemoteEditViewModel SelectedRemoteEditViewModel
        {
            get { return _selectedRemoteEditViewModel; }
            set
            {
                _selectedRemoteEditViewModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsChanged => RemoteEditViewModels.Any(f => f.Remote.IsChanged);

        private void OnAddRemoteExecute(object obj)
        {
            IRemoteEditViewModel remoteEditVm = _remoteEditViewModelCreator();
            RemoteEditViewModels.Add(remoteEditVm);
            remoteEditVm.Load();
            SelectedRemoteEditViewModel = remoteEditVm;
        }

        private void OnOpenRemoteTab(int remoteId)
        {
            IRemoteEditViewModel remoteEditVm = RemoteEditViewModels.SingleOrDefault(vm => vm.Remote.Id == remoteId);
            if (remoteEditVm == null)
            {
                remoteEditVm = _remoteEditViewModelCreator();
                RemoteEditViewModels.Add(remoteEditVm);
                var nav = NavigationViewModel as NavigationViewModel;
                remoteEditVm.Load(remoteId, nav.NavigationItems.SingleOrDefault(x => x.RemoteId == remoteId));
            }
            SelectedRemoteEditViewModel = remoteEditVm;
        }

        private void OnCloseRemoteTabExecute(object parameter)
        {
            var remoteEditVmToClose = parameter as IRemoteEditViewModel;
            if (remoteEditVmToClose != null)
            {
                if (remoteEditVmToClose.Remote.IsChanged)
                {
                    var result = System.Windows.MessageBox.Show("You'll lose your changes if you close this tab. Close it?", "Close tab?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result != System.Windows.MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                RemoteEditViewModels.Remove(remoteEditVmToClose);
            }
        }

        private void OnRemoteDeleted(int remoteId)
        {
            IRemoteEditViewModel remoteDetailVmToClose
              = RemoteEditViewModels.SingleOrDefault(f => f.Remote.Id == remoteId);
            if (remoteDetailVmToClose != null)
            {
                RemoteEditViewModels.Remove(remoteDetailVmToClose);
            }
        }

        private void OnPublishAllExecute(object obj)
        {
            OnSaveAllExecute(null);

            var nav = NavigationViewModel as NavigationViewModel;
            foreach (var item in nav.NavigationItems)
            {
                IRemoteEditViewModel remoteEditVm = RemoteEditViewModels.SingleOrDefault(vm => vm.Remote.Id == item.RemoteId);
                if (remoteEditVm == null)
                {
                    remoteEditVm = _remoteEditViewModelCreator();
                    remoteEditVm.Load(item.RemoteId, nav.NavigationItems.SingleOrDefault(x => x.RemoteId == item.RemoteId));
                    if (!remoteEditVm.Remote.Exclude)
                    {
                        RemoteEditViewModels.Add(remoteEditVm);
                        SelectedRemoteEditViewModel = remoteEditVm;
                    }
                }
            }

            foreach (var item in RemoteEditViewModels)
            {
                var remote = item.Remote;
                if (!item.Remote.Exclude)
                {
                    if (!Directory.Exists(remote.SourcePath))
                    {

                        remote.ProgressMessage = "Source path does not exists";
                        return;
                    }

                    remote.ProgressMessage = "Cleaning local folder...";
                    foreach (string file in Directory.GetFiles(remote.SourcePath, "*.tmp").Where(x => x.EndsWith(".tmp")))
                    {
                        File.Delete(file);
                    }

                    Directory.CreateDirectory(remote.SourcePath);

                    if (File.Exists(remote.SourcePath + @"\" + "app.zip"))
                    {
                        File.Delete(remote.SourcePath + @"\" + "app.zip");
                    }
                }
            }

            foreach (var item in RemoteEditViewModels)
            {
                if (!item.Remote.Exclude)
                {
                    item.Publish();
                }
            }
        }

        private void OnSaveAllExecute(object obj)
        {
            foreach (var item in RemoteEditViewModels)
            {
                item.Save();
            }
        }

        private void OnCancelAllExecute(object obj)
        {
            foreach (var item in RemoteEditViewModels)
            {
                Sync.ClearThreads();
                item.Abort();
            }
        }

        private void OnDeleteAllExecute(object obj)
        {

            var dialogResult = MessageBox.Show("Do you want delete all?", "Delete all", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                _remoteDataProvider.DeleteAll();
                NavigationViewModel.Load();
                this.RemoteEditViewModels.Clear();
            }

        }

    }
}

using HmiPublisher.Model;
using HmiPublisher.UI.Command;
using HmiPublisher.UI.DataProvider.Lookups;
using HmiPublisher.UI.Events;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace HmiPublisher.UI.ViewModel
{
    public interface INavigationViewModel
    {
        void Load();
    }

    public class NavigationViewModel : INavigationViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILookupProvider<Remote> _remoteLookupProvider;

        public NavigationViewModel(IEventAggregator eventAggregator, ILookupProvider<Remote> remoteLookupProvider)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<RemoteSavedEvent>().Subscribe(OnRemoteSaved);
            _eventAggregator.GetEvent<RemoteDeletedEvent>().Subscribe(OnRemoteDeleted);
            _remoteLookupProvider = remoteLookupProvider;
            NavigationItems = new ObservableCollection<NavigationItemViewModel>();
        }

        public void Load()
        {
            NavigationItems.Clear();

            var lookups = _remoteLookupProvider.GetLookup();

            if (lookups == null)
            {
                return;
            }

            foreach (var item in lookups)
            {
                NavigationItems.Add(new NavigationItemViewModel(item.Id, item.DisplayValue, _eventAggregator));
            }
        }

        public ObservableCollection<NavigationItemViewModel> NavigationItems { get; set; }

        private void OnRemoteDeleted(int remoteId)
        {
            var navigationItem = NavigationItems.SingleOrDefault(item => item.RemoteId == remoteId);
            if (navigationItem != null)
            {
                NavigationItems.Remove(navigationItem);
            }
        }

        private void OnRemoteSaved(Remote savedRemote)
        {
            var navigationItem = NavigationItems.SingleOrDefault(item => item.RemoteId == savedRemote.Id);

            if (navigationItem != null)
            {
                navigationItem.DisplayValue = string.Format("{0}", savedRemote.Name);
            }
            else
            {
                Load();
            }
        }
    }

    public class NavigationItemViewModel : Observable
    {
        private readonly IEventAggregator _eventAggregator;

        public NavigationItemViewModel(int remoteId, string displayValue, IEventAggregator eventAggregator)
        {
            RemoteId = remoteId;
            DisplayValue = displayValue;
            _eventAggregator = eventAggregator;
            OpenRemoteEditViewCommand = new DelegateCommand(OpenRemoteEditViewExecute);
        }

        public ICommand OpenRemoteEditViewCommand { get; set; }

        public int RemoteId { get; private set; }

        private string _displayValue;
        public string DisplayValue
        {
            get { return _displayValue; }
            set
            {
                _displayValue = value;
                OnPropertyChanged();
            }
        }

        private int _progressPercentage;
        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }
        private void OpenRemoteEditViewExecute(object obj)
        {
            _eventAggregator.GetEvent<OpenRemoteEditViewEvent>().Publish(RemoteId);
        }
    }
}

using Autofac;
using HmiPublisher.DataAccess;
using HmiPublisher.Model;
using HmiPublisher.UI.DataProvider;
using HmiPublisher.UI.DataProvider.Lookups;
using HmiPublisher.UI.View.Services;
using HmiPublisher.UI.ViewModel;
using Microsoft.Practices.Prism.PubSubEvents;

namespace HmiPublisher.UI.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();

            builder.RegisterType<FileDataService>().As<IDataService>();
            builder.RegisterType<RemoteLookupProvider>().As<ILookupProvider<Remote>>();
            builder.RegisterType<RemoteDataProvider>().As<IRemoteDataProvider>();

            builder.RegisterType<RemoteEditViewModel>().As<IRemoteEditViewModel>();
            builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>();
            builder.RegisterType<MainViewModel>().AsSelf();

            return builder.Build();
        }
    }
}

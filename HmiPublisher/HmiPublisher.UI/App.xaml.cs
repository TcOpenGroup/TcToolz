using System.Windows;
using Autofac;
using HmiPublisher.UI.Startup;
using HmiPublisher.UI.View;
using HmiPublisher.UI.ViewModel;
using HmiPublisher.UI.View.Services;

namespace HmiPublisher.UI
{
    public partial class App : Application
    {
        private MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new Bootstrapper();
            IContainer container = bootstrapper.Bootstrap();

            _mainViewModel = container.Resolve<MainViewModel>();
            MainWindow = new MainWindow(_mainViewModel);
            MainWindow.Show();
            _mainViewModel.Load();
        }
    }
}

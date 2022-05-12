using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using HmiPublisher.UI.ViewModel;
using Libs.Update;

namespace HmiPublisher.UI.View
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            new CheckForUpdateModel();

            var ver = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            
            Title = "HMI Publisher " + ver.Major +  "." + ver.Minor;

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _viewModel.OnClosing(e);
        }
    }
}

using Autofac;
using System;

namespace BackupNow
{
    public partial class ScanPage
    {
        public ScanPage()
        {
            InitializeComponent();
            DataContext = Bootstrapper.Container.Resolve<ScanViewModel>();
        }

        private void DataGridTextColumn_Opened(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}

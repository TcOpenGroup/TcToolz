using Autofac;
using ModernWpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BackupNow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);
            Title = App.GetTitle();
        }



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            DispatcherHelper.RunOnMainThread(() =>
            {
                if (this == Application.Current.MainWindow)
                {
                }
            });
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Bootstrapper.Container.Resolve<IEventAggregator>().GetEvent<KeyPressed>().Publish(KeyAction.Esc);
            }
            else if (e.Key == Key.F5)
            {
                Bootstrapper.Container.Resolve<IEventAggregator>().GetEvent<KeyPressed>().Publish(KeyAction.Refresh);
            }
            else if (e.Key == Key.F1)
            {
                Bootstrapper.Container.Resolve<IEventAggregator>().GetEvent<KeyPressed>().Publish(KeyAction.ShowDetails);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                DispatcherHelper.RunOnMainThread(() =>
                {
                    if (this == Application.Current.MainWindow)
                    {

                    }
                });
            }
        }

    }
}

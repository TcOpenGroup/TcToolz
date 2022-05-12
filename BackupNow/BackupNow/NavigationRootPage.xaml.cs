using Autofac;
using MaterialDesignThemes.Wpf;
using ModernWpf;
using ModernWpf.Controls;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Frame = ModernWpf.Controls.Frame;

namespace BackupNow
{
    public partial class NavigationRootPage
    {
        public static NavigationRootPage Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public static Frame RootFrame
        {
            get => _rootFrame.Value;
            private set => _rootFrame.Value = value;
        }

        private static readonly ThreadLocal<NavigationRootPage> _current = new ThreadLocal<NavigationRootPage>();
        private static readonly ThreadLocal<Frame> _rootFrame = new ThreadLocal<Frame>();

        private bool _ignoreSelectionChange;
        private readonly ControlPagesData _controlPagesData = new ControlPagesData();

        private MainViewModel _mainViewModel { get; set; }
        public NavigationRootPage()
        {
            InitializeComponent();

            Current = this;
            RootFrame = rootFrame;

            _mainViewModel = Bootstrapper.Container.Resolve<MainViewModel>();

            if (_mainViewModel.Settings.BackupItems.Count > 0)
            {
                PagesList.SelectedItem = PagesList.Items.OfType<ControlInfoDataItem>().FirstOrDefault(x => x.PageType == typeof(ScanPage));
            }
            else
            {
                PagesList.SelectedItem = PagesList.Items.OfType<ControlInfoDataItem>().FirstOrDefault(x => x.PageType == typeof(SettingsPage));
            }

            NavigateToSelectedPage();
        }

        private void NavigateToSelectedPage()
        {
            if (PagesList.SelectedValue is Type type)
            {
                RootFrame?.Navigate(type);
            }
        }

        private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ignoreSelectionChange)
            {
                NavigateToSelectedPage();
            }
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                RootFrame.RemoveBackEntry();
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            _ignoreSelectionChange = true;
            PagesList.SelectedValue = RootFrame.CurrentSourcePageType;
            _ignoreSelectionChange = false;
        }

        private void ForceGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    public class ControlPagesData : List<ControlInfoDataItem>
    {
        public ControlPagesData()
        {
            AddPage(typeof(ScanPage), PackIconKind.FolderSyncOutline, "Sync");
            AddPage(typeof(SettingsPage), PackIconKind.SettingsOutline, "Settings");
            AddPage(typeof(AboutPage), PackIconKind.InformationCircleOutline, "About");
        }

        private void AddPage(Type pageType, object icon, string displayName = null)
        {
            Add(new ControlInfoDataItem(pageType, icon, displayName));
        }
    }

    public class ControlInfoDataItem
    {
        public ControlInfoDataItem(Type pageType, object icon, string title = null)
        {
            PageType = pageType;
            Icon = icon;
            Title = title ?? pageType.Name.Replace("Page", null);
        }

        public object Icon { get; }
        public string Title { get; }

        public Type PageType { get; }

        public override string ToString()
        {
            return Title;
        }
    }
}

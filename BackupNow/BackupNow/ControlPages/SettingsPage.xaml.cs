﻿using Autofac;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace BackupNow
{
    public partial class SettingsPage
    {
        MainViewModel _mainViewModel { get; set; }

        public SettingsPage()
        {
            InitializeComponent();
            ThemeCombobox.SelectionChanged += ThemeCombobox_SelectionChanged;
            _mainViewModel = Bootstrapper.Container.Resolve<MainViewModel>();
            DataContext = _mainViewModel.Settings;
        }

        private void ThemeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = DataContext as AppSettingsWrapper;
            _mainViewModel.SetApplicationTheme(s.Theme);
        }

        private void OpenSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Utils.RunWithErrorHandling(() =>
            {
                Process.Start("explorer", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow\Settings.json");
            });
        }

        private void DataGrid_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            _mainViewModel.SaveSettings();
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            _mainViewModel.SaveSettings();
        }
    }
}

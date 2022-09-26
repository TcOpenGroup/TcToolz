﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuickConnect
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool Save;

        public SettingsWindow(SettingsWrapper model)
        {
            InitializeComponent();
            DataContext = model;

            PreviewKeyDown += new KeyEventHandler(HandleEsc);

            PathTextBox.Focus();

            Topmost = true;
        }


        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainViewModel.Instance.Settings = Utils.Clone(DataContext as SettingsWrapper);
            MainViewModel.Instance.SaveSettings();
        }
    }
}

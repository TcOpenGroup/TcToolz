﻿using QuickConnect.Data;
using System;
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
    public partial class AddWindow : Window
    {
        public ItemWrapper Item;
        public bool Save;
        public AddWindow()
        {
            InitializeComponent();
            Item = new ItemWrapper(new Item());
            DataContext = Item;

            PreviewKeyDown += new KeyEventHandler(HandleEsc);

            Name.Focus();

            Topmost = true;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Item = DataContext as ItemWrapper;
            Save = true;
            Close();
        }
    }
}

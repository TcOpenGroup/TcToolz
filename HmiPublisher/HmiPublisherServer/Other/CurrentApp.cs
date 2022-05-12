/*********************************************************
   Libs\Other\CurrentApp.cs
   
   Copyright (©) 2018 Marek Gvora
*********************************************************/

using System;
using System.Windows;

namespace Libs.Other
{
    public class CurrentApp
    {
        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public static string GetCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            ////string[] args = new string[] { "", @"C:\MTS\BindingTest\vortex.template.carousel\" };
            if (args != null)
            {
                int i = args.Length;
                MessageBox.Show(string.Join(", " , args));
                
                if (i == 2)
                {
                    return args[1];
                }
            }
            return "";
        }
    }
}

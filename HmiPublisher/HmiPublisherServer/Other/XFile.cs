/*********************************************************
   Libs\Other\XFile.cs
   
   Copyright (©) 2018 Marek Gvora
*********************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Libs.Other
{
    public class XFile
    {
        public static long GetFileLenght(string filePath)
        {
  
                return new FileInfo(filePath).Length;

         
        }

        public static bool Copy(string sourcePath, string destinationPath)
        {
            try
            {
                File.Copy(sourcePath, destinationPath, true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static string Delete(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string GetLine(int lineNumber, string fileName)
        {
            try
            {
                var lines = File.ReadAllLines(fileName);
                if (lineNumber <= lines.Length)
                {
                    for (int i = 0; i <= lines.Length; i++)
                    {
                        if (i == lineNumber - 1) { return lines[i]; }
                    }
                }
                else
                {
                    MessageBox.Show("Zadané číslo riadku nezodpovedá počtu riadkov v súbore");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return "";
        }
    }
}

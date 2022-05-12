
using System;
using System.IO;
using System.Windows;

namespace Libs.Other
{
    public class XFile
    {
        public static bool Copy(string sourcePath, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                }
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
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return "";
                }
                else
                {
                    return "";
                }

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Libs.Others
{
    public class CopyInformation
    {
        public string CurrentFile;
        public int ProgressPercentage;
        public int CopyProgress;
        public int TotalFileCount;

    }

    public class XDirectory
    {
        int fileCount;
        int copyProgress;
        double copyPercentage;
        CopyInformation information;
        public XDirectory()
        {
            information = new CopyInformation();
        }
        public delegate void CopyProgressEvent(object sender, CopyInformation e);
        public static event CopyProgressEvent CopyProgress;

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                try
                {
                    Directory.CreateDirectory(destDirName);
                }
                catch (Exception ex)
                {

                }

            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                copyPercentage = 100.0 / fileCount * copyProgress;
                information.ProgressPercentage = (int)copyPercentage;
                try
                {


                    copyProgress++;
                    information.CurrentFile = file.Name;
                    if (copyProgress == fileCount)
                    {
                        information.CurrentFile = "";
                        information.ProgressPercentage = 100;
                    }

                    file.CopyTo(temppath, true);
                }
                catch (Exception ex)
                {

                }
                CopyProgress(this, information);
            }
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        public static void DeleteAllFiles(string path, string exception)
        {

            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                //var containsException = exceptions.Any(o => o.Equals(file.Name));
                if (!file.Name.Contains(exception))
                {
                    System.IO.File.Delete(file.FullName);
                    //file.Delete();
                }
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

    }

}

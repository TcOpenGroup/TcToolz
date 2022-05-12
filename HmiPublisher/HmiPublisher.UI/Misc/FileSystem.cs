
using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Libs.Utils
{


    public static class FileSystem
    {
        public static long GetFilesLenghtInBytes(string directoryPath)
        {
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                        .Sum(t => (new FileInfo(t).Length));
        }
        public static long GetFileLenght(string filePath)
        {
            return new FileInfo(filePath).Length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFilePath"></param>
        /// <returns></returns>
        public static bool CopyFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
            }
            catch (Exception ex) { }
            if (File.Exists(destinationFilePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        public static void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void CreateHidenDir(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            try
            {
                di.Create();
                di.Attributes |= FileAttributes.Hidden;
            }
            catch (Exception ex) { }
        }
        public static void UpdateFileAttributesAndDeleteDir(DirectoryInfo dInfo)
        {
            if (Directory.Exists(dInfo.FullName))
            {
                dInfo.Attributes &= ~FileAttributes.ReadOnly;

                foreach (FileInfo file in dInfo.GetFiles())
                {
                    file.Attributes &= ~FileAttributes.ReadOnly;
                }

                foreach (DirectoryInfo subDir in dInfo.GetDirectories())
                {
                    UpdateFileAttributesAndDeleteDir(subDir);
                }

            }

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        /// <param name="exceptFiles"></param>
        public static void DeleteFolderContents(string sourceDir)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(sourceDir);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        /// <param name="exceptFiles"></param>
        public static void CopyAllFilesFromDirRecursively(string sourceDir, string targetDir)
        {

            // Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*",
                SearchOption.AllDirectories))
            {

                Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));


            }


            // Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourceDir, "*.*",
                SearchOption.AllDirectories))
            {

                File.Copy(newPath, newPath.Replace(sourceDir, targetDir), true);

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        /// <param name="exceptFiles"></param>
        public static void CopyAllFilesFromDir(string sourceDir, string targetDir, string exceptFiles)
        {
            string fileExt;
            string exceptFileExt = Path.GetExtension(exceptFiles);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                fileExt = Path.GetExtension(file);
                if (fileExt != exceptFiles || exceptFiles == "")
                {
                    try
                    {
                        File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
                    }
                    catch (Exception ec)
                    {
                    }
                }
            }
            //foreach (string directory in Directory.GetDirectories(sourceDir))
            //{
            //    File.Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        public static void WriteFile(string filePath, string content)
        {
            using (var output = new StreamWriter(filePath))
            {
                try
                {
                    output.WriteLine(content);
                    output.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var input = new StreamReader(filePath))
                {
                    try
                    {
                        return input.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        return "";
                    }
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="oldString"></param>
        /// <param name="newString"></param>
        /// <returns></returns>
        public static bool ReplaceStringInFile(string filePath, string oldString, string newString)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string text = File.ReadAllText(filePath);
                    text = text.Replace(oldString, newString);
                    File.WriteAllText(filePath, text);
                    if (File.ReadAllText(filePath).Contains(newString))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return false;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;

namespace GitMonitor
{
    public class Utils
    {
        public static string RunCommand(string workingDir, string cmd, bool sillent)
        {
            try
            {
                if (sillent)
                {
                    Process p = new Process();
                    p.StartInfo.WorkingDirectory = workingDir;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = "/c " + cmd;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    return output;
                }
                else
                {
                    ProcessStartInfo processInfo;
                    Process process;
                    processInfo = new ProcessStartInfo("cmd.exe");
                    processInfo.WorkingDirectory = workingDir;
                    processInfo.Arguments = "/c " + cmd;
                    processInfo.CreateNoWindow = false;
                    processInfo.UseShellExecute = false;
                    process = Process.Start(processInfo);
                    process.WaitForExit();
                    process.Close();
                    return "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "";
            }
        }


        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public static string GetCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null)
            {
                int i = args.Length;
                if (i == 2)
                {
                    return args[1];
                }
            }
            return "";
        }



        public static long GetFilesLenghtInBytes(string directoryPath)
        {
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                        .Sum(t => (new FileInfo(t).Length));
        }
        public static long GetFileLenght(string filePath)
        {
            return new FileInfo(filePath).Length;
        }
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


        public static void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

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

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

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

        public static string GetIpAddressFromString(string fullString)
        {
            try
            {
                Regex IPAd = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                MatchCollection MatchResult = IPAd.Matches(fullString);
                return MatchResult[0].Value;
            }
            catch (Exception ex) { return ""; }
        }

        public static bool PingHost(string ip)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(ip);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                pingable = false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
        public static bool CorrectIP(string ip)
        {
            if (String.IsNullOrWhiteSpace(ip))
            {
                return false;
            }
            string[] splitValues = ip.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }
            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        public static string CutIPAddress(string ip)
        {
            return ip.Substring(0, ip.LastIndexOf(".") + 1);
        }
    }
}

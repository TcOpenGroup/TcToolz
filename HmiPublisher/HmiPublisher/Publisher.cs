using HmiPublisher.Model;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HmiPublisher
{

    public class Publisher
    {
        const string Quote = "\"";
        static Mutex _spinLock = new Mutex();

        List<Thread> _tasks = new List<Thread>();

        bool CleanSourceTempFiles(RemoteWrapper item)
        {
            try
            {
                foreach (string file in Directory.GetFiles(item.FullSourcePath, "*.tmp").Where(x => x.EndsWith(".tmp")))
                {
                    File.Delete(file);
                }

                if (File.Exists(item.FullSourcePath + "app.zip"))
                {
                    File.Delete(item.FullSourcePath + "app.zip");
                }

                if (!File.Exists(item.FullSourcePath + "app.zip"))
                {
                    return false;
                }
                using (var x = File.Open(item.FullSourcePath + "app.zip", FileMode.Open))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                item.ProgressMessage = "Waiting for release source...";
                Thread.Sleep(50);
                CleanSourceTempFiles(item);
                return true;
            }
        }

        private string Build()
        {
            var msbuildpath = "";
            if (Directory.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin"))
            {
                msbuildpath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin";
            }
            else if (Directory.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"))
            {
                msbuildpath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";
            }
            else if (Directory.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\Current\Bin"))
            {
                msbuildpath = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\Current\Bin";
            }
            else if (Directory.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\Current\Bin"))
            {
                msbuildpath = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\Current\Bin";
            }
            else
            {
                return "";
            }

            msbuildpath = Quote + msbuildpath + @"\msbuild.exe" + Quote + " -verbosity:q /consoleloggerparameters:summary";

            var output = "";
            //if (MainViewModel.Instance.Settings.BuildConfiguration == BuildConfiguration.Debug)
            //{
            //    output = Utils.RunCommand(Utils.GetCommandLineArgs(), msbuildpath + " /p:Configuration=Debug", true);
            //}
            //else
            if (MainViewModel.Instance.Settings.BuildConfiguration == BuildConfiguration.Release)
            {
                output = Utils.RunCommand(Utils.GetCommandLineArgs(), msbuildpath + " /p:Configuration=Release", true);
            }
            else
            {
                output = "0 error";
            }
            return output;
        }

        void ShowBuildFailures(string value)
        {
            var filename = System.IO.Path.GetTempPath() + @"\HmiPublisher\buildLog.txt";
            using (StreamWriter readtext = new StreamWriter(filename))
            {
                readtext.Write(value);
            }

            try
            {
                Process.Start(filename);
            }
            catch (Exception)
            {
            }
        }

        public async Task PublishAsync(RemoteWrapper item)
        {
            try
            {
                item.InProgress = true;
                item.IndeterminateProgressBar = true;
                item.ProgressPercentage = 0;

                item.ProgressMessage = "Cleaning local folder...";
                item.IsError = false;
                await Task.Run(() =>
                  {
                      _tasks.Add(Thread.CurrentThread);

                      while (CleanSourceTempFiles(item)) { }

                      item.InProgress = true;
                      item.IndeterminateProgressBar = true;
                      item.ProgressPercentage = 0;
                      item.ProgressMessage = "Building solution...";
                      item.IsError = false;

                      var output = Build();
                      if (!output.ToLower().Contains("0 error"))
                      {
                          item.InProgress = false;
                          item.IndeterminateProgressBar = false;
                          item.ProgressPercentage = 0;
                          item.ProgressMessage = "Build failed";
                          item.IsError = true;
                          ShowBuildFailures(output);
                          return;
                      }

                      if (!Directory.Exists(item.FullSourcePath))
                      {
                          item.InProgress = false;
                          item.IndeterminateProgressBar = false;
                          item.ProgressPercentage = 0;
                          item.ProgressMessage = "Source path is invalid";
                          item.IsError = true;
                          return;
                      }

                      Publish(item);
                  });
            }
            catch (Exception ex)
            {

            }
        }

        public async Task PublishAllAync(IEnumerable<RemoteWrapper> items)
        {
            try
            {
                if (items.Count() == 0)
                {
                    return;
                }

                _spinLock = new Mutex();
                await Task.Run(() =>
                {
                    _tasks.Add(Thread.CurrentThread);
                    try
                    {
                        foreach (var item in items)
                        {
                            item.InProgress = true;
                            item.IndeterminateProgressBar = true;
                            item.ProgressPercentage = 0;
                            item.ProgressMessage = "Building solution...";
                            item.IsError = false;
                        }

                        var output = Build();
                        if (!output.ToLower().Contains("0 error"))
                        {
                            foreach (var item in items)
                            {
                                item.InProgress = false;
                                item.IndeterminateProgressBar = false;
                                item.ProgressPercentage = 0;
                                item.ProgressMessage = "Build failed";
                                item.IsError = true;
                            }

                            ShowBuildFailures(output);

                            return;
                        }

                        foreach (var item in items)
                        {
                            if (!Directory.Exists(item.FullSourcePath))
                            {
                                item.InProgress = false;
                                item.IndeterminateProgressBar = false;
                                item.ProgressPercentage = 0;
                                item.ProgressMessage = "Source path is invalid";
                                item.IsError = true;
                                return;
                            }

                            item.InProgress = true;
                            item.IndeterminateProgressBar = true;
                            item.ProgressPercentage = 0;
                            item.ProgressMessage = "Cleaning local folder...";
                            item.IsError = false;
                            foreach (string file in Directory.GetFiles(item.FullSourcePath, "*.tmp").Where(x => x.EndsWith(".tmp")))
                            {
                                File.Delete(file);
                            }

                            if (File.Exists(item.FullSourcePath + "app.zip"))
                            {
                                File.Delete(item.FullSourcePath + "app.zip");
                            }
                        }


                        Parallel.ForEach(items, new ParallelOptions { }, (item, state) =>
                         {
                             _tasks.Add(Thread.CurrentThread);
                             Publish(item);
                         });
                    }
                    catch (AggregateException ex)
                    {

                    }
                    catch (ThreadAbortException ex)
                    {

                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
            catch (ThreadAbortException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void Publish(RemoteWrapper item)
        {
            item.ProgressMessage = "Checking remote folder...";
            item.Compressing = false;
            item.IsError = false;
            if (!Directory.Exists(item.DestinationPath))
            {
                item.ProgressMessage = "Cannot get access to the remote folder";
                item.Compressing = false;
                item.IsError = true;
                item.IndeterminateProgressBar = false;
                item.InProgress = false;
                item.ProgressPercentage = 0;

                return;
            }

            var server = new PublisherServer();
            var command = "";
            var appProcessName = "";

            var initializeFailed = false;

            var t1 = Task.Run(() =>
            {
                _tasks.Add(Thread.CurrentThread);
                command = @"system; taskkill /f /im HmiPublisherServer.exe";
                var res = server.ConnectAndSend(item.IpAddress, command);
                if (!res)
                {
                    initializeFailed = true;
                    return;
                }

                if (Path.HasExtension(item.ExecutableFilePath))
                {
                    appProcessName = item.ExecutableFilePath.Substring(item.ExecutableFilePath.LastIndexOf(@"\") + 1);
                    command = @"system; taskkill /f /im " + appProcessName;
                    res = server.ConnectAndSend(item.IpAddress, command);
                    if (!res)
                    {
                        initializeFailed = true;
                        return;
                    }
                }

                if (Path.HasExtension(item.ExecutableFilePath))
                {
                    command = @"system; del /q /s " + "\"" + item.ExecutableFilePath.Substring(0, item.ExecutableFilePath.LastIndexOf(@"\") + 1) + "\"";
                }
                else
                {
                    command = @"system; del /q /s " + "\"" + item.ExecutableFilePath + "\"";
                }
                server.ConnectAndSend(item.IpAddress, command);

                Utils.RunCommand(@"net use \\" + item.IpAddress + " /user:" + item.TargetUserName + " " + item.TargetPass);

                var resx = Utils.Copy("HmiPublisherServer.exe", item.DestinationPath + "HmiPublisherServer.exe") && Utils.Copy("DotNetZip.dll", item.DestinationPath + "DotNetZip.dll");
                if (!resx)
                {
                    initializeFailed = true;
                    return;
                }

                while (File.Exists(item.DestinationPath + "app.zip")) { }
            });

            item.ProgressMessage = "Waiting for another task ...";
            item.IsError = false;
            if (_spinLock.WaitOne())
            {
                if (!File.Exists(item.FullSourcePath + "app.zip"))
                {
                    try
                    {
                        item.IndeterminateProgressBar = false;
                        item.Compressing = true;
                        item.ProgressMessage = "Compressing ...";
                        item.IsError = false;
                        //Package.CreateZipFile(item.FullSourcePath, item.FullSourcePath + "app.zip", item.Compression);
                        using (ZipFile zip = new ZipFile())
                        {
                            switch (MainViewModel.Instance.Settings.Compression)
                            {
                                case Compression.BestCompression:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                                    break;

                                case Compression.BestSpeed:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                                    break;

                                case Compression.Default:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                                    break;
                                case Compression.Level0:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level0;
                                    break;

                                case Compression.Level1:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level1;
                                    break;
                                case Compression.Level2:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level2;
                                    break;

                                case Compression.Level3:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level3;
                                    break;
                                case Compression.Level4:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level4;
                                    break;

                                case Compression.Level5:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level5;
                                    break;
                                case Compression.Level6:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level6;
                                    break;

                                case Compression.Level7:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level7;
                                    break;
                                case Compression.Level8:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level8;
                                    break;
                                //case Compression.Level9:
                                //    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level9;
                                //    break;

                                case Compression.None:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                                    break;
                                default:
                                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                                    break;
                            }

                            EventHandler<SaveProgressEventArgs> progressHandler = (o, i) =>
                            {
                                double copied = 100.0 / (double)i.EntriesTotal * (double)i.EntriesSaved;
                                int percentage = (int)copied;
                                if (percentage > 100)
                                {
                                    percentage = 100;
                                }
                                else if (percentage < 0)
                                {
                                    percentage = 0;
                                }

                                if (percentage == 0)
                                {
                                    return;
                                }

                                item.ProgressMessage = "Compressing files " + percentage.ToString() + "%";
                                item.ProgressPercentage = (int)percentage;
                            };

                            zip.SaveProgress += progressHandler;

                            zip.AddDirectory(item.FullSourcePath);
                            zip.Save(item.FullSourcePath + "app.zip");

                            zip.SaveProgress -= progressHandler;
                        }
                    }
                    catch (Exception)
                    {
                        _spinLock.ReleaseMutex();
                    }
                    finally
                    {
                        try
                        {
                            _spinLock.ReleaseMutex();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            item.IndeterminateProgressBar = true;
            item.Compressing = false;
            item.ProgressMessage = "Initializing remote machine ...";
            item.IsError = false;
            t1.Wait();

            if (initializeFailed)
            {
                item.ProgressMessage = "Initializing remote machine - FAILED";
                item.IsError = true;
                item.IndeterminateProgressBar = false;
                item.InProgress = false;
                item.ProgressPercentage = 0;
                return;
            }

            var totalFileLenghtBytes = Utils.GetFileLenght(item.FullSourcePath + "app.zip");

            if (Path.HasExtension(item.ExecutableFilePath))
            {
                command =
                  @"system; start " + item.ExecutableFilePath.Substring(0, item.ExecutableFilePath.LastIndexOf(@"\") + 1) +
                  "HmiPublisherServer.exe" +
                  " " +
                  totalFileLenghtBytes.ToString() +
                  " " +
                  "\"" + item.ExecutableFilePath + "\"" +
                  " " +
                  "\"" + appProcessName + "\"";
            }
            else
            {
                command =
                    @"system; start " + item.ExecutableFilePath +
                    @"\HmiPublisherServer.exe" +
                    " " +
                    totalFileLenghtBytes.ToString() +
                    " " +
                    "\"" + item.ExecutableFilePath + "\"" +
                    " " +
                    "\"" + appProcessName + "\"";
            }

            server.ConnectAndSend(item.IpAddress, command);

            var t2 = Task.Run(() =>
            {
                _tasks.Add(Thread.CurrentThread);
                while (true)
                {
                    try
                    {
                        var copiedFileLenghtBytes = Utils.GetFileLenght(item.DestinationPath + "app.zip");
                        var tmpProgressPercentage = 100.0 / totalFileLenghtBytes * copiedFileLenghtBytes;
                        var copiedFileLenghtMbytes = copiedFileLenghtBytes / 1000000;
                        var totalFileLenghtMbytes = totalFileLenghtBytes / 1000000;

                        item.ProgressPercentage = (int)tmpProgressPercentage;
                        item.IndeterminateProgressBar = false;
                        item.ProgressMessage = "Uploading " + item.ProgressPercentage.ToString() + "% (" + copiedFileLenghtMbytes.ToString() + " MB of " + totalFileLenghtMbytes.ToString() + " MB)";
                        item.IsError = false;
                        if (copiedFileLenghtBytes >= totalFileLenghtBytes)
                        {
                            item.ProgressPercentage = 100;
                            item.ProgressMessage = "Done";
                            item.IsError = false;
                            item.IndeterminateProgressBar = false;
                            item.InProgress = false;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            });

            var t3 = Task.Run(() =>
            {
                _tasks.Add(Thread.CurrentThread);
                using (Stream source = File.Open(item.FullSourcePath + "app.zip", FileMode.Open))
                {
                    using (Stream destination = File.Create(item.DestinationPath + "app.zip"))
                    {
                        source.CopyTo(destination);
                    }
                }
            });

            t3.Wait();
        }


        public async Task RestartApp(RemoteWrapper item)
        {
            var pub = new PublisherServer();

            if (!Path.HasExtension(item.ExecutableFilePath))
            {
                return;
            }

            var appProcessName = item.ExecutableFilePath.Substring(item.ExecutableFilePath.LastIndexOf(@"\") + 1);
            try
            {
                await Task.Run(() =>
                {
                    _tasks.Add(Thread.CurrentThread);
                    var command = @"system; taskkill /f /im " + appProcessName;
                    var res = pub.ConnectAndSend(item.IpAddress, command);

                    command = @"system; start " + item.ExecutableFilePath;
                    res = pub.ConnectAndSend(item.IpAddress, command);
                });
            }
            catch (Exception)
            {

            }
        }

        public async Task RestartAllApps(ObservableCollection<RemoteWrapper> items)
        {
            await Task.Run(() =>
            {
                _tasks.Add(Thread.CurrentThread);
                var pub = new PublisherServer();
                foreach (var item in items)
                {
                    if (!item.Include || !Path.HasExtension(item.ExecutableFilePath))
                    {
                        continue;
                    }

                    var appProcessName = item.ExecutableFilePath.Substring(item.ExecutableFilePath.LastIndexOf(@"\") + 1);
                    try
                    {
                        _tasks.Add(Thread.CurrentThread);
                        var command = @"system; taskkill /f /im " + appProcessName;
                        var res = pub.ConnectAndSend(item.IpAddress, command);

                        command = @"system; start " + item.ExecutableFilePath;
                        res = pub.ConnectAndSend(item.IpAddress, command);
                    }
                    catch (Exception)
                    {

                    }
                }
            });
        }

        public void Cancel()
        {
            try
            {
                foreach (var item in _tasks)
                {
                    item.Abort();
                }
                _tasks.Clear();
            }
            catch (Exception)
            {
            }
            try
            {
                _spinLock.ReleaseMutex();
            }
            catch (Exception)
            {
            }
        }

        private void UpdateUI(Action action)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                action();
            });
        }


    }
}

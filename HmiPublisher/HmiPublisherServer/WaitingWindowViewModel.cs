using Ionic.Zip;
using Libs.Other;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HmiPublisherServer
{
    public class WaitingWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string totalFileLenghtBytesInput { get; set; }
        private string filePathInput { get; set; }
        private string appProcessNameInput { get; set; }
        private long totalFileLenghtBytes { get; set; }
        private long totalFileLenght { get; set; }

        private int progressPercentage;
        public int ProgressPercentage
        {
            get { return this.progressPercentage; }
            set
            {
                this.progressPercentage = value;
                NotifyPropertyChanged(nameof(ProgressPercentage));
            }
        }
        private string progressMessage;
        public string ProgressMessage
        {
            get { return this.progressMessage; }
            set
            {
                this.progressMessage = value;
                NotifyPropertyChanged(nameof(ProgressMessage));
            }
        }

        private bool indeterminateProgressBar;
        public bool IndeterminateProgressBar
        {
            get { return this.indeterminateProgressBar; }
            set
            {
                this.indeterminateProgressBar = value;
                NotifyPropertyChanged(nameof(IndeterminateProgressBar));
            }
        }

        public WaitingWindowViewModel()
        {
            string[] args = Environment.GetCommandLineArgs();

            try
            {
                totalFileLenghtBytesInput = args[1];
                filePathInput = args[2];
                appProcessNameInput = args[3];


                var folder = "";
                if (Path.HasExtension(filePathInput))
                {
                    folder = Path.GetDirectoryName(filePathInput);
                }
                else
                {
                    folder = filePathInput;
                }

                while (!File.Exists(folder + @"\app.zip")) { }

                totalFileLenghtBytes = long.Parse(totalFileLenghtBytesInput);

                if (totalFileLenghtBytes > 0)
                {
                    ShowProgressAsync();
                }
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name + " - " + line);
            }
        }
        async void ShowProgressAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                ShowProgress();
            });
        }
        private void ShowProgress()
        {
            double tmpProgressPercentage;
            long copiedFileLenghtBytes = 0;
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory; // Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                                                                           //   var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\"; // Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(50);
                    copiedFileLenghtBytes = XFile.GetFileLenght(currentDir + @"app.zip");
                    tmpProgressPercentage = 100.0 / totalFileLenghtBytes * copiedFileLenghtBytes;

                    ProgressPercentage = (int)tmpProgressPercentage;

                    ProgressMessage = ProgressPercentage.ToString() + "%";

                    if (copiedFileLenghtBytes >= totalFileLenghtBytes)
                    {
                        IndeterminateProgressBar = false;
                        if (File.Exists(currentDir + @"app.zip"))
                        {
                            System.Threading.Thread.Sleep(1000);

                            //ProgressMessage = "Extracting files ...";
                            using (var zip = new ZipFile(currentDir + @"app.zip"))
                            {
                                zip.ExtractProgress += Zip_ExtractProgress;
                                zip.ExtractAll(currentDir, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }

                        try
                        {
                            IndeterminateProgressBar = false;
                            XFile.Delete(currentDir + @"app.zip");

                            if (Path.HasExtension(filePathInput))
                            {
                                ProgressMessage = "Starting application ...";
                                Process.Start(filePathInput);
                                System.Threading.Thread.Sleep(1500);
                            }
                        }
                        catch (Exception ex)
                        {
                            ProgressMessage = ex.Message;
                            break;
                        }

                        Process.GetCurrentProcess().Kill();
                    }
                }
                catch (Exception ex)
                {
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name + " - " + line);
                    break;
                }
            }
        }

        private void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            double copied = 100.0 / (double)e.EntriesTotal * (double)e.EntriesExtracted;
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

            ProgressMessage = "Extracting files " + percentage.ToString() + "%";
            ProgressPercentage = (int)percentage;

        }
    }
}

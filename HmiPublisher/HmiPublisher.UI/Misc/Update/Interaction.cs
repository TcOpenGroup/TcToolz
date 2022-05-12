namespace Libs.Update
{
    using System.ComponentModel;


    public class Interaction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int progressPercentage;
        public int ProgressPercentage
        {
            get { return this.progressPercentage; }
            set
            {
                this.progressPercentage = value;
                NotifyPropertyChanged("ProgressPercentage");
            }
        }
        private string percentageMessage;
        public string PercentageMessage
        {
            get { return this.percentageMessage; }
            set
            {
                this.percentageMessage = value;
                NotifyPropertyChanged("PercentageMessage");
            }
        }
        private string progressMessage;
        public string ProgressMessage
        {

            get { return this.progressMessage; }
            set
            {
                this.progressMessage = value;
                NotifyPropertyChanged("ProgressMessage");
            }
        }
        protected void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}

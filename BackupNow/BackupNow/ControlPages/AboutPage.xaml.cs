using Autofac;

namespace BackupNow
{
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();
            VersionLabel.Content = "Version: " + App.GetVersion();
        }
    }
}

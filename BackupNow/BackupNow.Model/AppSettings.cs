using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupNow.Model
{
    public enum AppTheme
    {
        [Description("Light")]
        Light,
        [Description("Dark")]
        Dark,
        [Description("System")]
        System
    }

    public class BackupItem
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public bool Enabled { get; set; } = true;
        public bool IsYearFolder { get; set; }
    }

    public class AppSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.System;
        public IEnumerable<BackupItem> BackupItems { get; set; } = new List<BackupItem>();
        public bool ShutdownWhenFinished { get; set; }
        public bool RunOnStartup { get; set; } = true;
    }
}

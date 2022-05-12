using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitMonitor.Model
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

    public class AppSettings
    {
        public ICollection<string> IgnoreList { get; set; } = new List<string>();
        public AppTheme Theme { get; set; } = AppTheme.System;
        public string DefaultFolder { get; set; }
    }
}

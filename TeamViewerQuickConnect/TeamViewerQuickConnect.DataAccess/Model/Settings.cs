using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConnect.Data
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

    public class Settings
    {
        public string TeamViewerPath { get; set; } = "";
        public string LastQuery { get; set; } = "";
        public bool CloseOnSubmit { get; set; } = false;
        public AppTheme Theme { get; set; } = AppTheme.System;
        public double SizeW { get; set; }
        public double SizeH { get; set; }

        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public bool RememberSize { get; set; } = true;
        public bool RememberLocation { get; set; } = true;
    }
}

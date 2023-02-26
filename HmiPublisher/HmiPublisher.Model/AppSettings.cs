using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPublisher.Model
{
    public enum BuildConfiguration
    {
        //[Description(nameof(Debug))]
        //Debug,
        [Description(nameof(Release))]
        Release,
        [Description(nameof(None))]
        None
    }

    public enum AppTheme
    {
        [Description(nameof(Light))]
        Light,
        [Description(nameof(Dark))]
        Dark,
        [Description(nameof(System))]
        System
    }

    public class AppSettings
    {
        public Compression Compression { get; set; } = Compression.Default;
        public BuildConfiguration BuildConfiguration { get; set; } = BuildConfiguration.Release;
        public AppTheme Theme { get; set; } = AppTheme.System;
        public bool ShutdownWhenFinished { get; set; } = true;
    }
}

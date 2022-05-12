
using BackupNow.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BackupNow
{
    public class AppSettingsWrapper : ModelWrapper<AppSettings>
    {
        public AppSettingsWrapper(AppSettings model) : base(model)
        {
        }

        public AppTheme Theme
        {
            get => GetValue<AppTheme>();
            set => SetValue(value);
        }

        public List<BackupItem> BackupItems
        {
            get => GetValue<List<BackupItem>>();
            set => SetValue(value);
        }

        public bool AutoScan
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public bool ShutdownWhenFinished
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
    }
}

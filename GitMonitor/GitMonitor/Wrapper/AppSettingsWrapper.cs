
using GitMonitor.Model;
using System.Collections.Generic;

namespace GitMonitor
{
    public class AppSettingsWrapper : ModelWrapper<AppSettings>
    {
        public AppSettingsWrapper(AppSettings model) : base(model)
        {
        }

        public ICollection<string> IgnoreList
        {
            get => GetValue<ICollection<string>>();
            set => SetValue(value);
        }

        public AppTheme Theme
        {
            get => GetValue<AppTheme>();
            set => SetValue(value);
        }

        public string DefaultFolder
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}

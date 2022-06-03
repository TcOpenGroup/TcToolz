
using QuickConnect.Data;

namespace QuickConnect
{
    public class SettingsWrapper : ModelWrapperBase<Settings>
    {
        public SettingsWrapper(Settings model) : base(model)
        {
        }

        public string TeamViewerPath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool CloseOnSubmit
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public AppTheme Theme
        {
            get => GetValue<AppTheme>();
            set => SetValue(value);
        }

        public string LastQuery
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}

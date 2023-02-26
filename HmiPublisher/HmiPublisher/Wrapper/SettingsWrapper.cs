using HmiPublisher.Model;

namespace HmiPublisher
{
    public class SettingsWrapper : ModelWrapper<AppSettings>
    {
        public SettingsWrapper(AppSettings model) : base(model)
        {
        }

        public Compression Compression
        {
            get => GetValue<Compression>();
            set => SetValue(value);
        }

        public BuildConfiguration BuildConfiguration
        {
            get => GetValue<BuildConfiguration>();
            set => SetValue(value);
        }

        //public BuildAction BuildAction
        //{
        //    get => GetValue<BuildAction>();
        //    set => SetValue(value);
        //}

        public AppTheme Theme
        {
            get => GetValue<AppTheme>();
            set => SetValue(value);
        }

        public bool ShutdownWhenFinished
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
    }
}

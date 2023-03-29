
using BackupNow.Model;

namespace BackupNow
{
    public class ItemWrapper : ModelWrapper<Item>
    {
        public ItemWrapper(Item model) : base(model)
        {
        }

        public string FileName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string SourcePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string SourceFilePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string DestinationPath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public long NewChange
        {
            get => GetValue<long>();
            set => SetValue(value);
        }

        public int Progress
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
    }
}

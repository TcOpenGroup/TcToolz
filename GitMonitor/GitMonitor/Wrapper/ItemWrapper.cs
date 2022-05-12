
using GitMonitor.Model;

namespace GitMonitor
{
    public class ItemWrapper : ModelWrapper<Item>
    {
        public ItemWrapper(Item model) : base(model)
        {
        }

        public string Dir
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int Changes
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public int UnpushedCommits
        {
            get => GetValue<int>();
            set => SetValue(value);
        }


        public bool Include
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
    }
}


using QuickConnect.Data;

namespace QuickConnect
{
    public class ItemWrapper : ModelWrapperBase<Item>
    {
        public ItemWrapper(Item model) : base(model)
        {
        }

        public int Id
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string TeamViewerID
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Password
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        private bool _online;
        public bool Online
        {
            get { return _online; }
            set
            {
                _online = value;
                OnPropertyChanged(nameof(Online));
            }
        }
    }
}

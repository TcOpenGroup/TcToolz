
using IPCONFIG.Model;

namespace IPCONFIG
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

        public string IP
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string SubnetMask
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Notes
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}

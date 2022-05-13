
using IPCONFIG.Model;

namespace IPCONFIG
{
    public class InterfaceWrapper : ModelWrapperBase<Interface>
    {
        public InterfaceWrapper(Interface model) : base(model)
        {
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}

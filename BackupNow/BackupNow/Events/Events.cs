using ModernWpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupNow
{
    public enum KeyAction
    {
        Refresh,
        ShowDetails,
        Esc,
    }

    public class KeyPressed : PubSubEvent<KeyAction> { }
    public class ThemeChanged : PubSubEvent<ApplicationTheme?> { }
}

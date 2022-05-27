using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcOpen.VisualStudio.Tools
{
    static class GuidList
    {
        public const string PackageGuidString = "F9946079-F715-4F4B-81B8-9CC773EDE92E";

        public static readonly Guid guidToolsCmdPkg = new Guid(PackageGuidString);
        public static readonly Guid guidToolsCmdSet = new Guid("961F40D1-143D-4CF8-9BA0-3F54BA445AAA");
    }
}

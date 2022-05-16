using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcOpenVSTools
{
    static class GuidList
    {
        public const string PackageGuidString = "a7fa87fe-7fb4-4027-a415-980f66fcbf5d";

        public const string guidDaxEditorPkgString = "80e7b569-e29a-4e63-ad9c-d5315887bc08";
        public const string guidDaxEditorCmdSetString = "72e9e9c7-4d84-4d58-9d0c-7cd63efdeb81";

        public static readonly Guid guidDaxEditorCmdSet = new Guid(guidDaxEditorCmdSetString);
    }
}

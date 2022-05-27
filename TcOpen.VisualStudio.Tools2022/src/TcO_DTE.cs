using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcOpen.VisualStudio.Tools
{
    public class TcO_DTE
    {
        private readonly DTE _dte;

        public TcO_DTE(DTE dte)
        {
            _dte = dte;
        }

        public string GetSolutionFilePath()
        {
            return !string.IsNullOrEmpty(_dte.Solution.FullName) ? _dte.Solution.FullName : "";
        }

        public string GetSolutionPath()
        {
            return Path.GetDirectoryName(GetSolutionFilePath());
        }

        public void ExecuteCommand(Command command)
        {
            switch (command)
            {
                case Command.SaveAll:
                    _dte.ExecuteCommand("File.SaveAll");
                    break;
                case Command.Build:
                    break;
                case Command.RebuildAll:
                    break;
                case Command.Copy:
                    _dte.ExecuteCommand("Edit.Copy");
                    break;
                case Command.Paste:
                    _dte.ExecuteCommand("Edit.Paste");
                    break;
            }
        }

    }

    public enum Command
    {
        SaveAll = 1,
        Cut = 10,
        Copy = 11,
        Paste = 12,
        Build = 20,
        RebuildAll = 21,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupNow.Model
{
    public class Item
    {
        public string FileName { get; set; }
        public string SourcePath { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationPath { get; set; }
        public long NewChange { get; set; }
        public int Progress { get; set; }

        public override string ToString()
        {
            return "Source: " + SourcePath + " Destination: " + DestinationPath;
        }
    }
}

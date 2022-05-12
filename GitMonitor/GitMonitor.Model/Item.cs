using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitMonitor.Model
{
    public class Item
    {
        public string Dir { get; set; }
        public int Changes { get; set; }
        public int UnpushedCommits { get; set; }
        public bool Include { get; set; }
    }
}

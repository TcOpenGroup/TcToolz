using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCONFIG.Model
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public string SubnetMask { get; set; }
        public string Notes { get; set; }
    }
}

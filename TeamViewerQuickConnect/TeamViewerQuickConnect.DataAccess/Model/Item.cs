using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConnect.Data
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TeamViewerID { get; set; }
        public string Password { get; set; }
    }

    public class ItemObsolete
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string Password { get; set; }
    }
}

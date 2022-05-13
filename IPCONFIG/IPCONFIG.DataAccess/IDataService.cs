using IPCONFIG.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCONFIG.DataAccess
{
    public interface IDataService
    {
        IEnumerable<Item> GetItems();
        void SaveItems(IEnumerable<Item> items);
        string FilePath();
    }
}

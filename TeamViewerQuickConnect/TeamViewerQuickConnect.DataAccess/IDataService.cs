using QuickConnect.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConnect.DataAccess
{
    public interface IDataService
    {
        IEnumerable<Item> GetItems();
        void SaveItems(IEnumerable<Item> items);
        string FilePath();

        Settings GetSettings();
        void SaveSettings(Settings item);
    }
}

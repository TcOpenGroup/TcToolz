using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConnect.Data
{
    public interface IDataService
    {
        void EnsureDbPathExists();
        void MigrateFromXml();

        void ExportToXML(string filepath, IEnumerable<Item> items);
        IEnumerable<Item> ImportFromXML(string filepath);

        void Add(Item item);
        void AddRange(List<Item> item);
        void Update(Item item);
        void Remove(Item item);
        void RemoveAll();
        void RemoveRange(IEnumerable<Item> items);
        IEnumerable<Item> GetItems();
        IEnumerable<Item> FindByName(string name);
        void SaveItems(IEnumerable<Item> items);
        string GetItemsFilePath();

        Settings GetSettings();
        void SaveSettings(Settings item);
    }
}

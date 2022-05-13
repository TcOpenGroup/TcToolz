using IPCONFIG.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCONFIG.DataAccess
{
    public class FileDataService : IDataService
    {
        private string _itemsFilePath;
        public FileDataService()
        {
            _itemsFilePath = Path.GetTempPath() + @"IPCONFIG\Items.json";
        }

        public IEnumerable<Item> GetItems()
        {
            if (!File.Exists(_itemsFilePath))
            {
                return new List<Item>();
            }

            string json = File.ReadAllText(_itemsFilePath);

            var data = JsonConvert.DeserializeObject<List<Item>>(json);

            if (data == null)
            {
                data = new List<Item>();
            }

            return data;
        }

        public void SaveItems(IEnumerable<Item> items)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_itemsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_itemsFilePath));
            }

            var i = 1;
            foreach (var item in items)
            {
                item.Id = i;
                i++;
            }
            string json = JsonConvert.SerializeObject(items, Formatting.Indented);
            File.WriteAllText(_itemsFilePath, json);
        }

        public string FilePath()
        {
            return _itemsFilePath;
        }
    }
}

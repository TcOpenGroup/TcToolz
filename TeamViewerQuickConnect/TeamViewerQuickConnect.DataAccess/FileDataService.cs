using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickConnect.Data
{
    public class FileDataService : IDataService
    {
        private string _settingsFilePath;
        private string _itemsFilePath;
        public FileDataService()
        {
            _settingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Settings.json";
            _itemsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\QuickConnect.db";
        }

        public void Add(Item item)
        {
            using (var db = new DataContext())
            {
                item.Id = 0;
                db.Items.Add(item);
                db.SaveChanges();
            }
        }

        public void AddRange(List<Item> item)
        {
            using (var db = new DataContext())
            {
                db.Items.AddRange(item);
                db.SaveChanges();
            }
        }

        public void Update(Item item)
        {
            using (var db = new DataContext())
            {
                db.Items.Update(item);
                db.SaveChanges();
            }
        }

        public void Remove(Item item)
        {
            using (var db = new DataContext())
            {
                db.Items.Remove(item);
                db.SaveChanges();
            }
        }

        public void RemoveAll()
        {
            using (var db = new DataContext())
            {
                db.Items.RemoveRange(db.Items);
                db.SaveChanges();
            }
        }

        public void RemoveRange(IEnumerable<Item> items)
        {
            using (var db = new DataContext())
            {
                db.Items.RemoveRange(items);
                db.SaveChanges();
            }
        }

        public IEnumerable<Item> GetItems()
        {
            var data = new List<Item>();

            using (var db = new DataContext())
            {
                data = db.Items
                    .OrderBy(b => b.Name)
                    .Take(30)
                    .ToList();
            }

            return data;
        }

        public IEnumerable<Item> FindByName(string name)
        {
            var data = new List<Item>();

            using (var db = new DataContext())
            {
                data = db.Items
                    .Where(b => b.Name.ToLower().Contains(name.ToLower()))
                    .OrderBy(b => b.Name)
                    .Take(30)
                    .ToList();
            }

            return data;
        }

        public void SaveItems(IEnumerable<Item> items)
        {
            using (var db = new DataContext())
            {
                db.AddRange(items);
                db.SaveChanges();
            }
        }

        public string GetItemsFilePath()
        {
            return _itemsFilePath;
        }

        public Settings GetSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return DefaultSettings();
            }

            string json = File.ReadAllText(_settingsFilePath);

            var data = JsonConvert.DeserializeObject<Settings>(json);

            if (data == null || string.IsNullOrEmpty(data.TeamViewerPath))
            {
                return DefaultSettings();
            }

            return data;
        }

        private Settings DefaultSettings()
        {
            var tvpath = "";
            if (File.Exists(@"C:\Program Files\TeamViewer\TeamViewer.exe"))
            {
                tvpath = @"C:\Program Files\TeamViewer\TeamViewer.exe";
            }
            else if (File.Exists(@"C:\Program Files (x86)\TeamViewer\TeamViewer.exe"))
            {
                tvpath = @"C:\Program Files (x86)\TeamViewer\TeamViewer.exe";
            }
            else
            {
                throw new TeamViewerNotFoundException();
            }
            return new Settings()
            {
                TeamViewerPath = tvpath,
            };
        }

        public void SaveSettings(Settings item)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));
            }
            string json = JsonConvert.SerializeObject(item, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }

        public void MigrateFromXml()
        {
            try
            {
                EnsureDbPathExists();

                if (File.Exists(Path.GetTempPath() + @"\TeamViewerQuickConnect\Settings.json") && !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Settings.json"))
                {
                    File.Copy(Path.GetTempPath() + @"\TeamViewerQuickConnect\Settings.json", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Settings.json", true);
                }

                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Items.json"))
                {
                    string json = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Items.json");

                    var data = JsonConvert.DeserializeObject<List<ItemObsolete>>(json);

                    if (data == null)
                    {
                        data = new List<ItemObsolete>();
                    }

                    foreach (var item in data)
                    {
                        Add(new Item() { Id = 0, Name = item.Name, TeamViewerID = item.ID, Password = item.Password });
                    }
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Items.json");
                    File.Delete(Path.GetTempPath() + @"\TeamViewerQuickConnect\Items.json");
                }
                else if (File.Exists(Path.GetTempPath() + @"\TeamViewerQuickConnect\Items.json"))
                {
                    string json = File.ReadAllText(Path.GetTempPath() + @"\TeamViewerQuickConnect\Items.json");

                    var data = JsonConvert.DeserializeObject<List<ItemObsolete>>(json);

                    if (data == null)
                    {
                        data = new List<ItemObsolete>();
                    }

                    foreach (var item in data)
                    {
                        Add(new Item() { Id = 0, Name = item.Name, TeamViewerID = item.ID, Password = item.Password });
                    }
                    File.Delete(Path.GetTempPath() + @"\TeamViewerQuickConnect\Items.json");
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void EnsureDbPathExists()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect");
            }
        }

        public void ExportToXML(string filepath, IEnumerable<Item> items)
        {
            string json = JsonConvert.SerializeObject(items, Formatting.Indented);
            File.WriteAllText(filepath, json);

        }

        public IEnumerable<Item> ImportFromXML(string filepath)
        {
            string json = File.ReadAllText(filepath);
            var data = JsonConvert.DeserializeObject<List<Item>>(json);

            foreach (var item in data)
            {
                item.Id = 0;
            }

            AddRange(data);

            return data;
        }


    }

    public class TeamViewerNotFoundException : ApplicationException
    {
    }
}

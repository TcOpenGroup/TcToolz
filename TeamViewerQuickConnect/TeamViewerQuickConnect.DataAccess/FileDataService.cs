using QuickConnect.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickConnect.DataAccess
{
    public class FileDataService : IDataService
    {
        private string _settingsFilePath;
        private string _itemsFilePath;
        public FileDataService()
        {
            try
            {
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect");
                    if (Directory.Exists(Path.GetTempPath() + @"\TeamViewerQuickConnect"))
                    {
                        if (File.Exists(Path.GetTempPath() + @"\TeamViewerQuickConnect\Settings.json"))
                        {
                            File.Copy(Path.GetTempPath() + @"\TeamViewerQuickConnect\Settings.json", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Settings.json", true);
                        }
                        if (File.Exists(Path.GetTempPath() + @"\TeamViewerQuickConnect\Items.json"))
                        {
                            File.Copy(Path.GetTempPath() + @"\TeamViewerQuickConnect\Items.json", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Items.json", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }


            _settingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Settings.json";

            var p = GetCommandLineArgs();
            if (string.IsNullOrEmpty(p))
            {
                _itemsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TeamViewerQuickConnect\Items.json";
            }
            else
            {
                _itemsFilePath = p + @"\QuickConnect.json";
            }
        }

        private string GetCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null)
            {
                int i = args.Length;
                if (i == 2)
                {
                    return args[1];
                }
            }
            return "";
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
    }

    public class TeamViewerNotFoundException : ApplicationException
    {
    }
}

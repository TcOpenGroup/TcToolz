using BackupNow.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BackupNow.DataAccess
{
    public class FileDataService : IDataService
    {
        private string _appSettingsFilePath = "";
        public FileDataService()
        {
            //if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow\Settings.json") && File.Exists(System.IO.Path.GetTempPath() + @"BackupNow\Settings.json"))
            //{
            //    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow"))
            //    {
            //        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow");
            //    }
            //    File.Copy(System.IO.Path.GetTempPath() + @"BackupNow\Settings.json", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow\Settings.json");
            //}
            _appSettingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BackupNow\Settings.json";
        }

        public AppSettings GetAppSettings()
        {
            if (!File.Exists(_appSettingsFilePath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(_appSettingsFilePath);

            var data = JsonConvert.DeserializeObject<AppSettings>(json);

            if (data == null)
            {
                return new AppSettings();
            }
            return data;
        }

        public void SaveAppSettings(AppSettings settings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_appSettingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_appSettingsFilePath));
            }

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_appSettingsFilePath, json);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

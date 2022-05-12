using HmiPublisher.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HmiPublisher.DataAccess
{
    public class FileDataService : IDataService
    {
        private string _storageFile = "Remotes.json";
        private string _appSettingsFilePath = "";
        private string _projectPath = "";
        public FileDataService()
        {
            _appSettingsFilePath = System.IO.Path.GetTempPath() + @"\HmiPublisher\Settings.json";
            _projectPath = GetCommandLineArgs();
            if (!string.IsNullOrEmpty(_projectPath))
            {
                _storageFile = _projectPath + @"\Remotes.json";
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

        [Obsolete]
        public Remote GetById(int remoteId)
        {
            var remote = ReadFromFile();
            return remote.Single(f => f.Id == remoteId);
        }


        [Obsolete]
        public void Save(Remote remote)
        {
            if (remote.Id <= 0)
            {
                InsertRemote(remote);
            }
            else
            {
                UpdateRemote(remote);
            }
        }

        [Obsolete]
        public void Delete(int remoteId)
        {
            var remote = ReadFromFile();
            var existing = remote.Single(f => f.Id == remoteId);
            remote.Remove(existing);
            SaveToFile(remote);
        }

        [Obsolete]
        public void DeleteAll()
        {
            var remotes = ReadFromFile();

            remotes.Clear();

            SaveToFile(remotes);


        }

        [Obsolete]
        private void UpdateRemote(Remote remote)
        {
            var remotes = ReadFromFile();
            var existing = remotes.Single(f => f.Id == remote.Id);
            var indexOfExisting = remotes.IndexOf(existing);
            remotes.Insert(indexOfExisting, remote);
            remotes.Remove(existing);
            SaveToFile(remotes);
        }

        [Obsolete]
        private void InsertRemote(Remote remotex)
        {
            var remote = ReadFromFile();

            var count = 0;

            if (remote.Count > 0)
            {
                count = remote.Max(f => f.Id);
            }


            remotex.Id = count + 1;
            remote.Add(remotex);

            SaveToFile(remote);
        }

        public IEnumerable<Remote> GetAll()
        {
            return ReadFromFile();
        }


        private void SaveToFile(List<Remote> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_storageFile, json);
        }

        private List<Remote> ReadFromFile()
        {
            if (!File.Exists(_storageFile))
            {
                return new List<Remote> { };
            }

            string json = File.ReadAllText(_storageFile);

            var data = JsonConvert.DeserializeObject<List<Remote>>(json);

            if (data == null)
            {
                data = new List<Remote> { };
            }

            var i = 1;
            foreach (var item in data)
            {
                item.Id = i;
                i++;
            }

            return data;
        }

        public string GetStorageFilePath()
        {
            return _projectPath;
        }

        public void SaveAll(IEnumerable<Remote> items)
        {
            var i = 1;
            foreach (var item in items)
            {
                item.Id = i;
                i++;
            }
            string json = JsonConvert.SerializeObject(items, Formatting.Indented);
            File.WriteAllText(_storageFile, json);
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

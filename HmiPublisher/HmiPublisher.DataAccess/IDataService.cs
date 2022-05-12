using HmiPublisher.Model;
using System;
using System.Collections.Generic;

namespace HmiPublisher.DataAccess
{
    public interface IDataService : IDisposable
    {
        [Obsolete]
        Remote GetById(int remoteId);
        [Obsolete]
        void Save(Remote remote);
        [Obsolete]
        void Delete(int remoteId);
        [Obsolete]
        void DeleteAll();
        IEnumerable<Remote> GetAll();
        void SaveAll(IEnumerable<Remote> items);
        string GetStorageFilePath();
        AppSettings GetAppSettings();
        void SaveAppSettings(AppSettings settings);

    }
}

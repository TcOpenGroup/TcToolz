using BackupNow.Model;
using System;
using System.Collections.Generic;

namespace BackupNow.DataAccess
{
    public interface IDataService : IDisposable
    {
        AppSettings GetAppSettings();
        void SaveAppSettings(AppSettings settings);
    }
}

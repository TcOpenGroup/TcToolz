using GitMonitor.Model;
using System;
using System.Collections.Generic;

namespace GitMonitor.DataAccess
{
    public interface IDataService : IDisposable
    {
        AppSettings GetAppSettings();
        void SaveAppSettings(AppSettings settings);
    }
}

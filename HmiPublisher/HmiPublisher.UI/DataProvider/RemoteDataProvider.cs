using HmiPublisher.DataAccess;
using HmiPublisher.Model;
using System;
using System.Collections.Generic;

namespace HmiPublisher.UI.DataProvider
{
    public class RemoteDataProvider : IRemoteDataProvider
    {
        private readonly Func<IDataService> _dataServiceCreator;

        public RemoteDataProvider(Func<IDataService> dataServiceCreator)
        {
            _dataServiceCreator = dataServiceCreator;
        }

        public Remote GetRemoteById(int id)
        {
            using (var dataService = _dataServiceCreator())
            {
                return dataService.GetById(id);
            }
        }
        public IEnumerable<Remote> GetAllRemotes()
        {
            using (var dataService = _dataServiceCreator())
            {
                return dataService.GetAll();
            }
        }

        public void SaveRemote(Remote remote)
        {
            using (var dataService = _dataServiceCreator())
            {
                dataService.Save(remote);
            }
        }

        public void DeleteRemote(int id)
        {
            using (var dataService = _dataServiceCreator())
            {
                dataService.Delete(id);
            }
        }

        public void DeleteAll()
        {
            using (var dataService = _dataServiceCreator())
            {

                dataService.DeleteAll();
            }
        }
    }
}

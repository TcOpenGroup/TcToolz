using HmiPublisher.Model;
using System.Collections.Generic;

namespace HmiPublisher.UI.DataProvider
{
    public interface IRemoteDataProvider
    {
        Remote GetRemoteById(int id);
        IEnumerable<Remote> GetAllRemotes();

        void SaveRemote(Remote friend);

        void DeleteRemote(int id);
        void DeleteAll();
    }
}
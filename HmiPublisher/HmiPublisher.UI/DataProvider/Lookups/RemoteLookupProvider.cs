using HmiPublisher.DataAccess;
using HmiPublisher.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HmiPublisher.UI.DataProvider.Lookups
{
    public class RemoteLookupProvider : ILookupProvider<Remote>
    {
        private readonly Func<IDataService> _dataServiceCreator;

        public RemoteLookupProvider(Func<IDataService> dataServiceCreator)
        {
            _dataServiceCreator = dataServiceCreator;
        }

        public IEnumerable<LookupItem> GetLookup()
        {
            using (var service = _dataServiceCreator())
            {
                return service.GetAll()?.Select(item => new LookupItem
                {
                    Id = item.Id,
                    DisplayValue = item.Name
                }).ToList();
            }
        }
    }
}
using System.Collections.Generic;

namespace HmiPublisher.UI.DataProvider.Lookups
{
    public interface ILookupProvider<T>
    {
        IEnumerable<LookupItem> GetLookup();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCenterControl.Services
{
    public interface IDataInterface<T>
    {
        Task<bool> CreateConnectionAsync();
        Task<bool> SendItemUpdateAsync(string id, string prop, string data);
        Task<bool> SubscribeItemUpdateAsync(string id, string type);
        Task<bool> UnSubscribeItemUpdate(string id);
        Task<bool> UnSubscribeAllItemsUpdate();
        Task<bool> GetItemsDefinitionAsync(bool forceRefresh = false);
        Task<bool> GetScheduleDataAsync();
        event EventHandler<string> DataReceived;
    }
}

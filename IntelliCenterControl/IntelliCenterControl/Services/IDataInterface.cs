using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliCenterControl.Services
{
    public interface IDataInterface<T>
    {
        Task<bool> CreateConnectionAsync();
        Task<bool> SendItemParamsUpdateAsync(string id, string prop, string data);
        Task<bool> SendItemCommandUpdateAsync(string id, string command, string data);
        Task<bool> SubscribeItemUpdateAsync(string id, string type);
        /// <summary>
        /// Subscribes a list of items at one time
        /// </summary>
        /// <param name="items">Item is name and type as string</param>
        /// <returns></returns>
        Task<bool> SubscribeItemsUpdateAsync(IDictionary<string , string> items);
        Task<bool> GetItemUpdateAsync(string id, string type);
        Task<bool> UnSubscribeItemUpdate(string id);
        Task<bool> UnSubscribeAllItemsUpdate();
        Task<bool> GetItemsDefinitionAsync(bool forceRefresh = false);
        Task<bool> GetScheduleDataAsync();
        event EventHandler<string> DataReceived;
        event EventHandler<T> ConnectionChanged;
        CancellationTokenSource Cts { get; set; }
    }
}

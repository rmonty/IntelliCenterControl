using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCenterControl.Services
{
    public interface IDataInterface<T>
    {
        Task<bool> UpdateItemAsync(string id, string prop, string data);
        void GetItemAsync(string id, string type);
        void GetItemsAsync(bool forceRefresh = false);
        event EventHandler<string> DataReceived;
    }
}

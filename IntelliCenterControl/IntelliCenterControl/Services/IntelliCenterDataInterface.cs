using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IntelliCenterControl.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntelliCenterControl.Services
{
    public class IntelliCenterDataInterface : IDataInterface<IntelliCenterConnection>
    {
        private HubConnection connection;
        private IntelliCenterConnection _intelliCenterConnection = new IntelliCenterConnection();
        public event EventHandler<string> DataReceived;
        public event EventHandler<IntelliCenterConnection> ConnectionChanged;

        public Dictionary<string, string> Subscriptions = new Dictionary<string, string>();
        public Dictionary<Guid, string> UnsubscribeMessages = new Dictionary<Guid, string>();

        public IntelliCenterDataInterface()
        {
            
            
        }

        public async Task<bool> CreateConnectionAsync()
        {
            //connection = new HubConnectionBuilder()
            //    .WithUrl("ws://192.168.0.114:6680/")
            //    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20) })
            //    .Build();

            connection = new HubConnectionBuilder()
                .WithUrl(Settings.ServerURL)
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20) })
                .Build();

            connection.KeepAliveInterval = TimeSpan.FromSeconds(5);

            connection.Reconnecting += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Reconnecting);
                _intelliCenterConnection.State = (IntelliCenterConnection.ConnectionState)connection.State;
                OnConnectionChanged();
                // Notify users the connection was lost and the client is reconnecting.
                // Start queuing or dropping messages.

                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                Debug.Assert(connection.State == HubConnectionState.Connected);
                _intelliCenterConnection.State = (IntelliCenterConnection.ConnectionState)connection.State;
                OnConnectionChanged();
                // Notify users the connection was reestablished.
                // Start dequeuing messages queued while reconnecting if any.

                return Task.CompletedTask;
            };

            connection.Closed += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Disconnected);
                _intelliCenterConnection.State = (IntelliCenterConnection.ConnectionState)connection.State;
                OnConnectionChanged();
                // Notify users the connection has been closed or manually try to restart the connection.

                return Task.CompletedTask;
            };

            await connection.StartAsync();

            if(connection.State == HubConnectionState.Connected) DataSubscribe();
            
            _intelliCenterConnection.State = (IntelliCenterConnection.ConnectionState)connection.State;
            OnConnectionChanged();

            return await Task.FromResult(connection.State != HubConnectionState.Disconnected);
        }

        public async Task<bool> SendItemUpdateAsync(string id, string prop, string data)
        {
            var message = CreateParameters(id, prop, data);

            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    if (connection.State == HubConnectionState.Connected)
                    {
                        await connection.InvokeAsync("Request", message);
                        return await Task.FromResult(true);
                        //Console.WriteLine(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return await Task.FromResult(false);
        }

        public async Task<bool> GetScheduleDataAsync()
        {
            var g = Guid.NewGuid();
            var cmd =
                "{ \"command\": \"GETPARAMLIST\", \"condition\": \"OBJTYP=SCHED\", \"objectList\": [{ \"objnam\": \"ALL\", \"keys\": " + Schedule.ScheduleKeys + " }], \"messageID\": \"" +
                g + "\" }";

            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    await connection.InvokeAsync("Request", cmd);
                    return await Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            return await Task.FromResult(false);
        }

        
        public async Task<bool> SubscribeItemUpdateAsync(string id, string type)
        {
            if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(type, out var result))
            {
                var g = Guid.NewGuid();
                var key = String.Empty;

                switch (result)
                {
                    case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                        key = Pump.PumpKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                        key = Body.BodyKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                        key = Sense.SenseKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.CIRCUIT:
                        key = Circuit<IntelliCenterConnection>.CircuitKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                        key = Circuit<IntelliCenterConnection>.CircuitKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                        key = Circuit<IntelliCenterConnection>.CircuitKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                        key = Chem.ChemKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                        key = Heater.HeaterKeys;
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                    case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                    case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                    case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                    case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                    case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                    case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                        key = Light.LightKeys;
                        break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    try
                    {
                        Subscriptions[id] = key;
                        var cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + key + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        if (connection.State == HubConnectionState.Connected)
                        {
                            await connection.InvokeAsync("Request", cmd);
                            return await Task.FromResult(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> UnSubscribeItemUpdate(string id)
        {
            if(Subscriptions.TryGetValue(id, out var keys))
            {
                var g = Guid.NewGuid();
                var cmd =
                    "{ \"command\": \"ReleaseParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                    "\", \"keys\": " + keys + " }], \"messageID\": \"" +
                    g + "\" }";

                if (!string.IsNullOrEmpty(cmd))
                {
                    try
                    {
                        if (connection.State == HubConnectionState.Connected)
                        {
                            await connection.InvokeAsync("Request", cmd);
                            UnsubscribeMessages[g] = id;
                            return await Task.FromResult(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> UnSubscribeAllItemsUpdate()
        {
            if (Subscriptions.Count > 0)
            {
                var g = Guid.NewGuid();
                var cmd =
                    "{ \"command\": \"ClearParam\", \"messageID\": \"" + g + "\" }";

                if (!string.IsNullOrEmpty(cmd))
                {
                    try
                    {
                        if (connection.State == HubConnectionState.Connected)
                        {
                            await connection.InvokeAsync("Request", cmd);
                            return await Task.FromResult(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            return await Task.FromResult(false);
        }

        protected virtual void OnConnectionChanged()
        {
            EventHandler<IntelliCenterConnection> handler = ConnectionChanged;
            handler?.Invoke(this, _intelliCenterConnection);
        }

        protected virtual void OnDataReceived(string message)
        {
            EventHandler<string> handler = DataReceived;
            handler?.Invoke(this, message);
        }

        public async Task<bool> GetItemsDefinitionAsync(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                var g = Guid.NewGuid();
                var cmd =
                    "{ \"command\": \"GetQuery\", \"queryName\": \"GetHardwareDefinition\", \"arguments\": \" \", \"messageID\": \"" +
                    g.ToString() + "\" }";
                try
                {
                    if (connection.State == HubConnectionState.Connected)
                    {
                        await connection.InvokeAsync("Request", cmd);
                        return await Task.FromResult(true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return await Task.FromResult(false);
        }

        

        private string CreateParameters(string objName, string property, string value)
        {
            var g = Guid.NewGuid();
            var paramsObject = "\"" + property + "\":\"" + value + "\"";

            var newobj = "{ \"objnam\": \"" + objName + "\", \"params\": {" + paramsObject + "}}";

            var message = "{ \"command\": \"SETPARAMLIST\", \"objectList\":[" + newobj + "], \"messageID\" : \"" + g.ToString() +"\" }";

            return message;
        }

        private async void DataSubscribe()
        {
            var stream = await connection.StreamAsChannelAsync<string>("Feed");
            while (await stream.WaitToReadAsync())
            {
                while (stream.TryRead(out var count))
                {
                    try
                    {
                        if (count.StartsWith('{'))
                        {
                            var data = JsonConvert.DeserializeObject(count);
                            if (data != null)
                            {
                                var jData = (JObject) data;
                                if (jData.TryGetValue("command", out var commandValue))
                                {
                                    switch (commandValue.ToString())
                                    {
                                        case "ClearParam":
                                            Subscriptions.Clear();
                                            UnsubscribeMessages.Clear();
                                            break;
                                        case "ReleaseParamList":
                                            if (jData.TryGetValue("messageID", out var g))
                                            {
                                                var gid = (Guid)g;
                                                if (UnsubscribeMessages.TryGetValue(gid, out var id))
                                                {
                                                    Subscriptions.Remove(id);
                                                    UnsubscribeMessages.Remove(gid);
                                                }
                                            }
                                            break;
                                        default:
                                            OnDataReceived(count);
                                            break;
                                    }
                                }
                            }

                            //Console.WriteLine($"{count}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}
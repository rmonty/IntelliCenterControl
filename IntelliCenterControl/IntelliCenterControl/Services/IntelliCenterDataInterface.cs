using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IntelliCenterControl.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;

namespace IntelliCenterControl.Services
{
    public class IntelliCenterDataInterface : IDataInterface<HardwareDefinition>
    {
        HubConnection connection;
        
        public event EventHandler<string> DataReceived;
        
        public const string pumpKeys = "[\"RPM\", \"GPM\", \"PWR\",\"STATUS\"]";
        public const string bodyKeys = "[\"TEMP\",\"STATUS\",\"HTMODE\",\"MODE\",\"LSTTMP\"]";
        public const string senseKeys = "[\"PROBE\", \"STATUS\"]";
        public const string circuitKeys = "[\"STATUS\", \"MODE\"]";
        public const string chemKeys = "[\"SALT\"]";
        

        public IntelliCenterDataInterface()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://192.168.0.130:5000/stream")
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20) })
                .Build();

            connection.KeepAliveInterval = TimeSpan.FromSeconds(5);

            connection.Reconnecting += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Reconnecting);

                // Notify users the connection was lost and the client is reconnecting.
                // Start queuing or dropping messages.

                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                Debug.Assert(connection.State == HubConnectionState.Connected);

                // Notify users the connection was reestablished.
                // Start dequeuing messages queued while reconnecting if any.

                return Task.CompletedTask;
            };

            connection.Closed += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Disconnected);

                // Notify users the connection has been closed or manually try to restart the connection.

                return Task.CompletedTask;
            };


            connection.StartAsync();
            DataSubscribe();
        }

        public async Task<bool> UpdateItemAsync(string id, string prop, string data)
        {
            var message = CreateParameters(id, prop, data);

            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    await connection.InvokeAsync("Request", message);
                    //Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return await Task.FromResult(true);
        }

        
        public async void GetItemAsync(string id, string type)
        {
            if (Enum.TryParse<Circuit.CircuitType>(type, out var result))
            {
                var g = Guid.NewGuid();
                var cmd = String.Empty;

                switch (result)
                {
                    case Circuit.CircuitType.PUMP:
                        cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + pumpKeys + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        break;
                    case Circuit.CircuitType.BODY:
                        cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + bodyKeys + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        break;
                    case Circuit.CircuitType.SENSE:
                        cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + senseKeys + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        break;
                    case Circuit.CircuitType.CIRCUIT:
                        cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + circuitKeys + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        break;
                    case Circuit.CircuitType.CIRCGRP:
                        cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + circuitKeys + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        break;
                    case Circuit.CircuitType.CHEM:
                        cmd =
                            "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                            "\", \"keys\": " + chemKeys + " }], \"messageID\": \"" +
                            g.ToString() + "\" }";
                        break;
                    default: break;
                }

                if (!string.IsNullOrEmpty(cmd))
                {
                    try
                    {
                        await connection.InvokeAsync("Request", cmd);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        protected virtual void OnDataReceived(string message)
        {
            EventHandler<string> handler = DataReceived;
            handler?.Invoke(this, message);
        }

        public async void GetItemsAsync(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                var g = Guid.NewGuid();
                var cmd =
                    "{ \"command\": \"GetQuery\", \"queryName\": \"GetHardwareDefinition\", \"arguments\": \" \", \"messageID\": \"" +
                    g.ToString() + "\" }";
                try
                {
                    await connection.InvokeAsync("Request", cmd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        

        private string CreateParameters(string objName, string property, string value)
        {
            var g = Guid.NewGuid();
            var paramsObject = "\"" + property + "\":\"" + value + "\"";

            var newobj = "{ \"objnam\": \"" + objName + "\", \"params\": [" + paramsObject + "]}";

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
                        if (!count.StartsWith('p'))
                        {
                            //HardwareDefinition = await Task.FromResult(JsonSerializer.Deserialize<HardwareDefinition>(count, jsonOptions));
                            OnDataReceived(count);
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
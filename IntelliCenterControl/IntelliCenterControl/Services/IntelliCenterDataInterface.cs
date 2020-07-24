using IntelliCenterControl.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliCenterControl.Services
{
    public class IntelliCenterDataInterface : IDataInterface<IntelliCenterConnection>
    {
        private ILogService _logService;
        private readonly ICloudLogService _cloudLogService;
        private Settings _settings = Settings.Instance;

        private HubConnection connection;
        private ClientWebSocket socketConnection;
        private IntelliCenterConnection _intelliCenterConnection = new IntelliCenterConnection();
        private readonly SemaphoreSlim _sendRateLimit = new SemaphoreSlim(1);
        private TimeSpan _sendRate = new TimeSpan(0, 0, 0, 0, 50);

        public event EventHandler<string> DataReceived;
        public event EventHandler<IntelliCenterConnection> ConnectionChanged;

        public Dictionary<string, string> Subscriptions = new Dictionary<string, string>();
        public Dictionary<Guid, string> UnsubscribeMessages = new Dictionary<Guid, string>();

        public CancellationTokenSource Cts { get; set; }

        private int tokenExpiration;

        private Timer _tokenExpireTimer;

        public IntelliCenterDataInterface(ILogService logService, ICloudLogService cloudLogService)
        {
            _logService = logService;
            _cloudLogService = cloudLogService;
            Cts = new CancellationTokenSource();
            _tokenExpireTimer = new Timer(TokenTimeOut);
        }

        private async Task<string> CheckCredentials()
        {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var client = new HttpClient(httpClientHandler);
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username", _settings.Username),
                new KeyValuePair<string, string>("password", _settings.Password),
                new KeyValuePair<string, string>("grant_type", "password")
            });
            var serverUrl = _settings.ServerURL;
            if (serverUrl.EndsWith(@"/")) serverUrl = serverUrl.Remove(serverUrl.LastIndexOf(@"/"));

            var response = await client.PostAsync(serverUrl + @"/Account/Token", content, Cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var fieldsCollector = new JsonFieldsCollector(JToken.Parse(json));
                var fields = (Dictionary<string, JValue>)fieldsCollector.GetAllFields();
                if (fields != null)
                {
                    if (fields.TryGetValue("access_token", out var token) && fields.TryGetValue("expires_in", out var expire))
                    {
                        _tokenExpireTimer.Dispose();
                        tokenExpiration = int.Parse(expire.Value.ToString());
                        _tokenExpireTimer = new Timer(TokenTimeOut, this, TimeSpan.FromSeconds(0),
                            TimeSpan.FromSeconds(tokenExpiration));
                        return token.ToString();
                    }
                }
                return null;
            }
            else
                return null;
        }

        private void TokenTimeOut(object state)
        {
            //CheckCredentials();
        }

        public async Task<bool> CreateConnectionAsync()
        {
            try
            {


                if (connection != null && connection.State == HubConnectionState.Connected)
                {
                    await connection.StopAsync();
                }

                if (socketConnection != null && socketConnection.State == WebSocketState.Open)
                {
                    await socketConnection.CloseAsync(WebSocketCloseStatus.NormalClosure, "", Cts.Token);
                }

                Cts?.Cancel();
                Cts?.Dispose();
                Cts = new CancellationTokenSource();

                _intelliCenterConnection.State = IntelliCenterConnection.ConnectionState.Disconnected;
                OnConnectionChanged();

                if (_settings.ServerURL.StartsWith("http"))
                {
                    var serverUrl = _settings.ServerURL;
                    if (serverUrl.EndsWith("/")) serverUrl += @"stream/";
                    else serverUrl += @"/stream/";

                    connection = new HubConnectionBuilder()
                        .WithUrl(serverUrl, options =>
                            {
                                options.AccessTokenProvider = async () => await CheckCredentials();
                                options.HttpMessageHandlerFactory = (message) =>
                                {
                                    if (message is HttpClientHandler clientHandler)
                                        // bypass SSL certificate
                                        clientHandler.ServerCertificateCustomValidationCallback +=
                                            (sender, certificate, chain, sslPolicyErrors) => true;
                                    return message;
                                };
                            })
                            .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20) })
                        .Build();

                    
                    connection.KeepAliveInterval = TimeSpan.FromSeconds(10);

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

                    await connection.StartAsync(Cts.Token);

                    if (connection.State == HubConnectionState.Connected) DataSubscribe();

                    _intelliCenterConnection.State = (IntelliCenterConnection.ConnectionState)connection.State;

                    OnConnectionChanged();
                }
                else if (_settings.ServerURL.StartsWith("ws"))
                {

                    socketConnection = new ClientWebSocket();

                    await socketConnection.ConnectAsync(new Uri(_settings.ServerURL), Cts.Token);

                    Thread.Sleep(50);

                    if (socketConnection.State == WebSocketState.Open) DataSubscribe();

                    switch (socketConnection.State)
                    {
                        case WebSocketState.Aborted:
                        case WebSocketState.Closed:
                        case WebSocketState.CloseReceived:
                        case WebSocketState.CloseSent:
                        case WebSocketState.None:
                            _intelliCenterConnection.State = IntelliCenterConnection.ConnectionState.Disconnected;
                            break;
                        case WebSocketState.Connecting:
                            _intelliCenterConnection.State = IntelliCenterConnection.ConnectionState.Connecting;
                            break;
                        case WebSocketState.Open:
                            _intelliCenterConnection.State = IntelliCenterConnection.ConnectionState.Connected;
                            break;
                        default:
                            _intelliCenterConnection.State = IntelliCenterConnection.ConnectionState.Disconnected;
                            break;
                    }

                    OnConnectionChanged();
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex);
                if (ex.Message.Contains("Unauthorized"))
                {
                    DataReceived?.Invoke(this, "Unauthorized");
                }
                //this._logService.LogError(ex.ToString());
                //this._cloudLogService.LogError(ex);
            }



            return await Task.FromResult(_intelliCenterConnection.State != IntelliCenterConnection.ConnectionState.Disconnected);
        }

        public async Task<bool> SendItemParamsUpdateAsync(string id, string prop, string data)
        {
            var message = CreateParameters(id, prop, data);

            if (!string.IsNullOrEmpty(message))
            {
                return (await SendMessage(message));
            }

            return await Task.FromResult(false);
        }

        public async Task<bool> SendItemCommandUpdateAsync(string id, string command, string data)
        {
            var message = CreateCommand(id, command, data);

            if (!string.IsNullOrEmpty(message))
            {
                return (await SendMessage(message));
            }

            return await Task.FromResult(false);
        }

        public Task<bool> GetScheduleDataAsync()
        {
            var g = Guid.NewGuid();
            var cmd =
                "{ \"command\": \"GETPARAMLIST\", \"condition\": \"OBJTYP=SCHED\", \"objectList\": [{ \"objnam\": \"ALL\", \"keys\": " + Schedule.ScheduleKeys + " }], \"messageID\": \"" +
                g + "\" }";

            return (SendMessage(cmd));
        }

        public async Task<bool> GetItemUpdateAsync(string id, string type)
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
                }

                if (!string.IsNullOrEmpty(key))
                {
                    Subscriptions[id] = key;
                    var cmd =
                        "{ \"command\": \"GetParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                        "\", \"keys\": " + key + " }], \"messageID\": \"" +
                        g.ToString() + "\" }";
                    return (await SendMessage(cmd));
                }
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
                }

                if (!string.IsNullOrEmpty(key))
                {
                    Subscriptions[id] = key;
                    var cmd =
                        "{ \"command\": \"RequestParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                        "\", \"keys\": " + key + " }], \"messageID\": \"" +
                        g.ToString() + "\" }";
                    return (await SendMessage(cmd));
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> SubscribeItemsUpdateAsync(IDictionary<string, string> items)
        {
            string message = "";

            foreach (var kvp in items)
            {
                if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(kvp.Value, out var result))
                {

                    switch (result)
                    {
                        case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Pump.PumpKeys + " }";
                            Subscriptions[kvp.Key] = Pump.PumpKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Body.BodyKeys + " }";
                            Subscriptions[kvp.Key] = Body.BodyKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Sense.SenseKeys + " }";
                            Subscriptions[kvp.Key] = Sense.SenseKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCUIT:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Circuit<IntelliCenterConnection>.CircuitKeys + " }";
                            Subscriptions[kvp.Key] = Circuit<IntelliCenterConnection>.CircuitKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Circuit<IntelliCenterConnection>.CircuitKeys + " }";
                            Subscriptions[kvp.Key] = Circuit<IntelliCenterConnection>.CircuitKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Circuit<IntelliCenterConnection>.CircuitKeys + " }";
                            Subscriptions[kvp.Key] = Circuit<IntelliCenterConnection>.CircuitKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Chem.ChemKeys + " }";
                            Subscriptions[kvp.Key] = Chem.ChemKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Heater.HeaterKeys + " }";
                            Subscriptions[kvp.Key] = Heater.HeaterKeys;
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                        case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                        case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                        case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                        case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                        case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                        case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                            if (!string.IsNullOrEmpty(message)) message += ",";
                            message += "{ \"objnam\": \"" + kvp.Key +
                                       "\", \"keys\": " + Light.LightKeys + " }";
                            Subscriptions[kvp.Key] = Light.LightKeys;
                            break;
                    }
                }

            }

            if (!string.IsNullOrEmpty(message))
            {
                var g = Guid.NewGuid();
                var cmd =
                    "{ \"command\": \"RequestParamList\", \"objectList\": [" + message + "], \"messageID\": \"" +
                    g.ToString() + "\" }";

                return (await SendMessage(cmd));
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> UnSubscribeItemUpdate(string id)
        {
            if (Subscriptions.TryGetValue(id, out var keys))
            {
                var g = Guid.NewGuid();
                var cmd =
                    "{ \"command\": \"ReleaseParamList\", \"objectList\": [{ \"objnam\": \"" + id +
                    "\", \"keys\": " + keys + " }], \"messageID\": \"" +
                    g + "\" }";

                return (await SendMessage(cmd));
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

                return (await SendMessage(cmd));
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

                return (await SendMessage(cmd));
            }
            return await Task.FromResult(false);
        }



        private string CreateParameters(string objName, string property, string value)
        {
            var g = Guid.NewGuid();
            var paramsObject = "\"" + property + "\":\"" + value + "\"";

            var newobj = "{ \"objnam\": \"" + objName + "\", \"params\": {" + paramsObject + "}}";

            var message = "{ \"command\": \"SETPARAMLIST\", \"objectList\":[" + newobj + "], \"messageID\" : \"" + g.ToString() + "\" }";

            return message;
        }

        private string CreateCommand(string objName, string methodName, string value)
        {
            var g = Guid.NewGuid();
            var argsObject = "\"" + objName + "\":\"" + value + "\"";

            var newobj = "\"method\": \"" + methodName + "\", \"arguments\": {" + argsObject + "}";

            var message = "{ \"command\": \"SETCOMMAND\", " + newobj + ", \"messageID\" : \"" + g.ToString() + "\" }";

            return message;
        }

        public async Task<bool> SendMessage(string message)
        {
            try
            {
                if (connection != null && connection.State == HubConnectionState.Connected)
                {
                    await connection.InvokeAsync("Request", message, Cts.Token);
                    return await Task.FromResult(true);
                }
                else if (socketConnection != null && socketConnection.State == WebSocketState.Open)
                {
                    // Wait for any previous send commands to finish and release the semaphore
                    // This throttles our commands
                    await _sendRateLimit.WaitAsync(Cts.Token);
                    ArraySegment<byte> byteMessage = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await socketConnection.SendAsync(byteMessage, WebSocketMessageType.Text, true, Cts.Token);
                    // Block other commands until our timeout to prevent flooding
                    await Task.Delay(_sendRate, Cts.Token);
                    // Exit our semaphore
                    _sendRateLimit.Release();
                    return await Task.FromResult(true);
                }
            }
            catch (Exception)
            {
                //this._logService.LogError(ex.ToString());
                //this._cloudLogService.LogError(ex);
            }

            return await Task.FromResult(false);
        }

        private async void DataSubscribe()
        {
            if (connection != null && connection.State == HubConnectionState.Connected)
            {
                try
                {
                    var stream = await connection.StreamAsChannelAsync<string>("Feed", Cts.Token);

                    while (connection != null && connection.State == HubConnectionState.Connected &&
                           await stream.WaitToReadAsync(Cts.Token))
                    {
                        while (stream.TryRead(out var count))
                        {
                            try
                            {
                                if (count.StartsWith("{"))
                                {
                                    var data = JsonConvert.DeserializeObject(count);
                                    if (data != null)
                                    {
                                        var jData = (JObject)data;
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
                                }
                            }
                            catch (Exception e)
                            {
                                this._logService.LogError(e.ToString());
                                this._cloudLogService.LogError(e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    this._logService.LogError(e.ToString());
                    //this._cloudLogService.LogError(e);
                }
            }
            else if (socketConnection != null && socketConnection.State == WebSocketState.Open)
            {
                try
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        while (socketConnection.State == WebSocketState.Open)
                        {
                            await ReadMessage();
                        }
                    }, Cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                catch (Exception)
                {
                    //this._logService.LogError(e.ToString());
                    //this._cloudLogService.LogError(e);
                }
            }
        }

        private async Task ReadMessage()
        {
            var message = new ArraySegment<byte>(new byte[4096]);
            try
            {
                await using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socketConnection.ReceiveAsync(message, Cts.Token);
                    if (result.MessageType != WebSocketMessageType.Text)
                        return;
                    ms.Write(message.Array ?? throw new InvalidOperationException(), message.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                var receivedMessage = reader.ReadToEnd();
                if (receivedMessage.StartsWith("{"))
                {
                    var data = JsonConvert.DeserializeObject(receivedMessage);
                    if (data != null)
                    {
                        var jData = (JObject)data;
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
                                    OnDataReceived(receivedMessage);
                                    break;
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {
                //this._logService.LogError(e.ToString());
                //this._cloudLogService.LogError(e);
            }

        }
    }
}
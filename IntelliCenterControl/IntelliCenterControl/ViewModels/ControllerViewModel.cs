using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using IntelliCenterControl.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using IntelliCenterControl.Services;

namespace IntelliCenterControl.ViewModels
{
    public class ControllerViewModel : BaseViewModel<IntelliCenterConnection>
    {
        private readonly ILogService _logService;
        private readonly ICloudLogService _cloudLogService;
        private Settings _settings = Settings.Instance;

        private HardwareDefinition _hardwareDefinition = new HardwareDefinition();

        public HardwareDefinition HardwareDefinition
        {
            get => _hardwareDefinition;
            set => SetProperty(ref _hardwareDefinition, value);
        }

        private SchedulesDefinition _scheduleDefinition = new SchedulesDefinition();

        public SchedulesDefinition ScheduleDefinition
        {
            get => _scheduleDefinition;
            set => SetProperty(ref _scheduleDefinition, value);
        }


        public Command LoadHardwareDefinitionCommand { get; set; }
        public Command ClosingCommand { get; set; }
        public Command SubscribeDataCommand { get; set; }
        public Command AllLightsOnCommand { get; set; }
        public Command AllLightsOffCommand { get; set; }
        public Command AddScheduleItemCommand { get; set; }


        
        public ObservableCollection<Circuit<IntelliCenterConnection>> Circuits { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> CircuitGroup { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Pumps { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Bodies { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Chems { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Lights { get; private set; }
        public ObservableCollection<Heater> Heaters { get; private set; }
        public ObservableCollection<Heater> ScheduleHeaters { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> TodaysSchedule { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Schedules { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> AvailableCircuits { get; private set; }
        public ConcurrentDictionary<string, Circuit<IntelliCenterConnection>> HardwareDictionary = new ConcurrentDictionary<string, Circuit<IntelliCenterConnection>>();

        private Circuit<IntelliCenterConnection> _airSensor;

        public Circuit<IntelliCenterConnection> AirSensor
        {
            get => _airSensor;
            set => SetProperty(ref _airSensor, value);
        }


        private Circuit<IntelliCenterConnection> _waterSensor;

        public Circuit<IntelliCenterConnection> WaterSensor
        {
            get => _waterSensor;
            set => SetProperty(ref _waterSensor, value);
        }

        private Circuit<IntelliCenterConnection> _solarSensor;

        public Circuit<IntelliCenterConnection> SolarSensor
        {
            get => _solarSensor;
            set => SetProperty(ref _solarSensor, value);
        }


        private bool _chemInstalled;

        public bool ChemInstalled
        {
            get => _chemInstalled;
            set => SetProperty(ref _chemInstalled, value);

        }

        private bool _hasCircuits;

        public bool HasCircuits
        {
            get => _hasCircuits;
            set => SetProperty(ref _hasCircuits, value);
        }

        private bool _hasCircuitGroups;

        public bool HasCircuitGroups
        {
            get => _hasCircuitGroups;
            set => SetProperty(ref _hasCircuitGroups, value);
        }

       

        private DateTime _currentDateTime;

        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value);
        }

        private Timer _dateTimeTimer;

        private string _statusMessage;

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage == value || String.IsNullOrEmpty(value)) return;
                _statusMessage = value;
                OnPropertyChanged();
            }
        }


        public ControllerViewModel(ILogService logService, ICloudLogService cloudLogService)
        {
            _logService = logService;
            _cloudLogService = cloudLogService;
            Title = "Pool Control";
            Circuits = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            CircuitGroup = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Pumps = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Bodies = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Chems = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Lights = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Heaters = new ObservableCollection<Heater>();
            ScheduleHeaters = new ObservableCollection<Heater>();
            TodaysSchedule = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Schedules = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            AvailableCircuits = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            LoadHardwareDefinitionCommand = new Command(async () => await ExecuteLoadHardwareDefinitionCommand());
            ClosingCommand = new Command(async () => await ExecuteClosingCommand());
            SubscribeDataCommand = new Command(async () => await ExecuteSubscribeDataCommand());
            AllLightsOnCommand = new Command(async () => await ExecuteAllLightsOnCommand());
            AllLightsOffCommand = new Command(async () => await ExecuteAllLightsOffCommand());
            AddScheduleItemCommand = new Command(ExecuteAddScheduleItemCommand);
            DataInterface.DataReceived += DataStoreDataReceived;
            DataInterface.ConnectionChanged += DataInterface_ConnectionChanged;
            _dateTimeTimer = new Timer(DateTimeTimerElapsed, this, 0, 1000);

            BindingBase.EnableCollectionSynchronization(AvailableCircuits, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Circuits, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(CircuitGroup, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Pumps, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Bodies, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Chems, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Lights, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Heaters, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(ScheduleHeaters, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(TodaysSchedule, null, ObservableCollectionCallback);
            BindingBase.EnableCollectionSynchronization(Schedules, null, ObservableCollectionCallback);

        }

        void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }

        private void ExecuteAddScheduleItemCommand()
        {
            if (Schedules.Any())
            {
                if (((Schedule)Schedules.ElementAt(Schedules.Count - 1)).IsNew)
                {
                    return;
                }
            }

            Device.BeginInvokeOnMainThread(
                () =>
                {
                    if (ScheduleHeaters.Any())
                    {

                        
                        Schedules.Add(new Schedule(Circuit<IntelliCenterConnection>.CircuitType.SCHED, DataInterface)
                        {
                            SelectedHeater = ScheduleHeaters[0],
                            IsNew = true,
                            Bodies = Bodies.ToList()
                        });
                    }
                    else
                    {
                        Schedules.Add(new Schedule(Circuit<IntelliCenterConnection>.CircuitType.SCHED, DataInterface)
                        {
                            IsNew = true,
                            Bodies = Bodies.ToList()
                        });
                    }
                });

        }

        private void DataInterface_ConnectionChanged(object sender, IntelliCenterConnection e)
        {
            if (e.State == IntelliCenterConnection.ConnectionState.Connected)
            {
                DataInterface.UnSubscribeAllItemsUpdate();
                DataInterface.GetItemsDefinitionAsync(true);
            }
            else
            {
                Device.BeginInvokeOnMainThread(
                    () =>
                    {
                        Circuits.Clear();
                        CircuitGroup.Clear();
                        Pumps.Clear();
                        Bodies.Clear();
                        Chems.Clear();
                        Lights.Clear();
                        Heaters.Clear();
                        ScheduleHeaters.Clear();
                        Schedules.Clear();
                        TodaysSchedule.Clear();
                    });
            }
        }

        public async Task UpdateIpAddress()
        {
            await ExecuteClosingCommand();
            await DataInterface.CreateConnectionAsync();
            await ExecuteLoadHardwareDefinitionCommand();
        }

        private static void DateTimeTimerElapsed(object state)
        {
            var cvm = (ControllerViewModel)state;
            if (cvm != null) cvm.CurrentDateTime = DateTime.Now;
        }

        private async Task ExecuteAllLightsOnCommand()
        {
            await DataInterface.SendItemParamsUpdateAsync("_A111", "STATUS", "ON");
        }

        private async Task ExecuteAllLightsOffCommand()
        {
            await DataInterface.SendItemParamsUpdateAsync("_A110", "STATUS", "OFF");
        }

        private Guid _hardwareDefinitionMessageId;
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private async void DataStoreDataReceived(object sender, string e)
        {
            if (!string.IsNullOrEmpty(e))
            {
                var data = JsonConvert.DeserializeObject(e);
                var jData = (JObject)(data);

                if (jData.TryGetValue("command", out var commandValue))
                {
                    try
                    {
                        switch (commandValue.ToString())
                        {
                            case "SendQuery":
                                if (jData.TryGetValue("queryName", out var queryNameValue))
                                {
                                    if (queryNameValue.ToString() == "GetHardwareDefinition")
                                    {
                                        if (jData.TryGetValue("messageID", out var g))
                                        {
                                            if (Guid.TryParse(g.ToString(), out var tempGuid))
                                            {
                                                if (_hardwareDefinitionMessageId != tempGuid)
                                                {
                                                    if (Guid.TryParse(g.ToString(), out _hardwareDefinitionMessageId))
                                                    {
                                                        StatusMessage = "Loading Data";
                                                        //Debug.WriteLine(e);
                                                        HardwareDefinition =
                                                            JsonConvert.DeserializeObject<HardwareDefinition>(e);

                                                        LoadModels();
                                                        PopulateModels();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case "NotifyList":
                            case "SendParamList":
                                if (jData.TryGetValue("objectList", out var objectListValue))
                                {
                                    var jDataArray = (JArray)objectListValue;

                                    if (jDataArray != null)
                                    {
                                        foreach (var jToken in jDataArray)
                                        {
                                            var notifyList = (JObject)jToken;
                                            if (notifyList.TryGetValue("objnam", out var objName))
                                            {
                                                if (HardwareDictionary.TryGetValue(objName.ToString(), out var circuit))
                                                {
                                                    switch (circuit.CircuitDescription)
                                                    {
                                                        case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                                                            var pump = (Pump)circuit;
                                                            pump.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                                                            var body = (Body)circuit;
                                                            body.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                                                            var sensor = (Sense)circuit;
                                                            sensor.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                                                            circuit.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                                                            circuit.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                                                            var chem = (Chem)circuit;
                                                            chem.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                                                            var light = (Light)circuit;
                                                            light.UpdateItemAsync(notifyList);
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                                                            //if (notifyList.TryGetValue("params", out var heaterValues))
                                                            //{
                                                            //    //Debug.WriteLine(heaterValues);
                                                            //    //var hv = (JObject)heaterValues;
                                                            //    //var heaterCircuit = (Heater)circuit;
                                                            //    //if (hv.TryGetValue("BODY", out var bodies))
                                                            //    //{

                                                            //    //}
                                                            //}

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (notifyList.TryGetValue("params", out var paramValues))
                                                    {
                                                        var paramArray = (JObject)paramValues;
                                                        if (paramArray.TryGetValue("OBJTYP", out var objType))
                                                        {
                                                            if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(objType.ToString(),
                                                                out var cktType))
                                                            {
                                                                switch (cktType)
                                                                {
                                                                    case Circuit<IntelliCenterConnection>.CircuitType.SCHED:
                                                                        StatusMessage = "Retrieving Schedule";
                                                                        try
                                                                        {
                                                                            await semaphoreSlim.WaitAsync(
                                                                                TimeSpan.FromSeconds(5));
                                                                            Device.BeginInvokeOnMainThread(
                                                                                () =>
                                                                                {
                                                                                    TodaysSchedule.Clear();
                                                                                    Schedules.Clear();
                                                                                });
                                                                            ScheduleDefinition =
                                                                                JsonConvert
                                                                                    .DeserializeObject<
                                                                                        SchedulesDefinition>(e);
                                                                            foreach (var sch in ScheduleDefinition
                                                                                .objectList)
                                                                            {
                                                                                var cirName = sch.Params.CIRCUIT;

                                                                                if (HardwareDictionary.TryGetValue(
                                                                                    cirName, out var schedCir))
                                                                                {
                                                                                    string htrName;

                                                                                    htrName =
                                                                                        sch.Params.HEATER == "HOLD"
                                                                                            ? "00001"
                                                                                            : sch.Params.HEATER;

                                                                                    var selectedHeater =
                                                                                        ScheduleHeaters.FirstOrDefault(
                                                                                            o =>
                                                                                                o.Hname == htrName);

                                                                                    var s = new Schedule(
                                                                                        sch.Params.SNAME,
                                                                                        Circuit<IntelliCenterConnection>
                                                                                            .CircuitType.SCHED,
                                                                                        sch,
                                                                                        schedCir,
                                                                                        sch.objnam,
                                                                                        DataInterface)
                                                                                    {
                                                                                        SelectedHeater = selectedHeater,
                                                                                        Bodies = Bodies.ToList()
                                                                                    };
                                                                                    Device.BeginInvokeOnMainThread(
                                                                                        () =>
                                                                                        {
                                                                                            Schedules.InsertInPlace(s,
                                                                                                o => o.ListOrd);
                                                                                        });

                                                                                    if (Today(sch.Params.DAY))
                                                                                    {
                                                                                        Device.BeginInvokeOnMainThread(
                                                                                            () =>
                                                                                            {
                                                                                                TodaysSchedule
                                                                                                    .InsertInPlace(s,
                                                                                                        o => o.ListOrd);
                                                                                            });

                                                                                    }

                                                                                }
                                                                            }
                                                                        }
                                                                        catch
                                                                        {
                                                                            // ignored
                                                                        }
                                                                        finally
                                                                        {
                                                                            semaphoreSlim.Release();
                                                                        }
                                                                        return;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                                break;
                            case "ObjectCreated":
                                StatusMessage = "Item Created";
                                break;
                            case "WriteParamList":
                                var WriteParamListJson = JToken.Parse(e);
                                var WriteParamListFieldsCollector = new JsonFieldsCollector(WriteParamListJson);
                                var WriteParamListFields = (Dictionary<string,JValue>)WriteParamListFieldsCollector.GetAllFields();
                                if (WriteParamListFields != null)
                                {
                                    bool itemAdded = false;

                                    if (WriteParamListFields.TryGetValue("objectList[0].deleted[0]", out var deletedValue))
                                    { 
                                        var deletedItem = (JValue)deletedValue;

                                        StatusMessage = "Item Deleted";
                                        try
                                        {
                                            await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(5));
                                            var schItem = Schedules.FirstOrDefault(o =>
                                                o.Hname == deletedItem.Value.ToString());
                                            if (schItem != null)
                                            {
                                                Schedules.Remove(schItem);
                                            }

                                            var todayItem = TodaysSchedule.FirstOrDefault(o =>
                                                o.Hname == deletedItem.Value.ToString());
                                            if (todayItem != null)
                                            {
                                                TodaysSchedule.Remove(todayItem);
                                            }
                                        }
                                        catch
                                        {
                                            // ignored
                                        }
                                        finally
                                        {
                                            semaphoreSlim.Release();
                                        }

                                        //DataInterface.GetScheduleDataAsync();
                                    }

                                    if (WriteParamListFields.TryGetValue("objectList[0].created[0].params.OBJTYP",
                                        out var createdObjectType))
                                    {
                                        if (createdObjectType.Value.ToString().Contains("SCHED"))
                                        {
                                            if (jData.TryGetValue("objectList", out var writeParamObjList))
                                            {
                                                var jDataArray = (JArray)writeParamObjList;
                                                if (jDataArray != null && jDataArray.HasValues)
                                                {
                                                    var createdJItem = jDataArray.FirstOrDefault(o=>o.ToString().Contains("created"));
                                                    if (createdJItem != null)
                                                    {
                                                        var createdJObject = (JObject) createdJItem;
                                                        if (createdJObject.TryGetValue("created", out var createdObject))
                                                        {

                                                            SchedulesDefinition.Schedule createdSchedule = null;
                                                            createdSchedule =
                                                                JsonConvert
                                                                    .DeserializeObject<SchedulesDefinition.Schedule>(
                                                                        createdObject.First.ToString());

                                                            if (Schedules.Any() && createdSchedule?.Params != null)
                                                            {
                                                                var newItem = (Schedule) Schedules[^1];
                                                                if (newItem.IsNew)
                                                                {

                                                                    if (HardwareDictionary.TryGetValue(
                                                                        createdSchedule.Params.CIRCUIT,
                                                                        out var schedCir))
                                                                    {
                                                                        Schedules.Remove(newItem);

                                                                        var selectedHeater =
                                                                            ScheduleHeaters.FirstOrDefault(o =>
                                                                                o.Hname == createdSchedule.Params
                                                                                    .HEATER);

                                                                        var sitem = new Schedule(
                                                                            createdSchedule.Params.SNAME,
                                                                            Circuit<IntelliCenterConnection>.CircuitType
                                                                                .SCHED,
                                                                            createdSchedule, schedCir,
                                                                            createdSchedule.objnam,
                                                                            DataInterface)
                                                                        {
                                                                            SelectedHeater = selectedHeater,
                                                                            Bodies = Bodies.ToList()
                                                                        };

                                                                        StatusMessage = "Item Added";
                                                                        Device.BeginInvokeOnMainThread(
                                                                            () =>
                                                                            {
                                                                                Schedules.InsertInPlace(sitem,
                                                                                    o => o.ListOrd);
                                                                            });

                                                                        if (Today(createdSchedule.Params.DAY))
                                                                        {
                                                                            Device.BeginInvokeOnMainThread(
                                                                                () =>
                                                                                {
                                                                                    TodaysSchedule.InsertInPlace(sitem,
                                                                                        o => o.ListOrd);
                                                                                });

                                                                        }

                                                                        itemAdded = true;

                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    
                                                }
                                            }
                                            
                                        }
                                    }

                                    if (WriteParamListFields.TryGetValue("objectList[0].changes[0].params.OBJTYP",
                                        out var changedObjectType))
                                    {
                                        if (jData.TryGetValue("objectList", out var writeParamObjList))
                                        {
                                            var jDataArray = (JArray)writeParamObjList;
                                            if (jDataArray != null && jDataArray.HasValues)
                                            {
                                                var changesItem = jDataArray.FirstOrDefault(o =>
                                                    o.ToString().Contains("changes"));
                                                if (changesItem != null)
                                                {
                                                    var changesJObject = (JObject)changesItem;
                                                    if (changesJObject.TryGetValue("changes",
                                                        out var changesObject))
                                                    {
                                                        if (changedObjectType.Value.ToString().Contains("SCHED"))
                                                        {
                                                            SchedulesDefinition.Schedule changedSchedule = null;
                                                            changedSchedule =
                                                                JsonConvert
                                                                    .DeserializeObject<SchedulesDefinition.Schedule>(
                                                                        changesObject.First.ToString());

                                                            if (Schedules.Any() && changedSchedule?.Params != null)
                                                            {
                                                                var changedItem =
                                                                    (Schedule)Schedules.FirstOrDefault(o =>
                                                                       o.Hname == changedSchedule.objnam);
                                                                if (changedItem != null)
                                                                {
                                                                    Schedules.Remove(changedItem);

                                                                    if (TodaysSchedule.Contains(changedItem))
                                                                    {
                                                                        TodaysSchedule.Remove(changedItem);
                                                                    }

                                                                    if (HardwareDictionary.TryGetValue(
                                                                        changedSchedule.Params.CIRCUIT,
                                                                        out var schedCir))
                                                                    {
                                                                        var selectedHeater =
                                                                            ScheduleHeaters.FirstOrDefault(o =>
                                                                                o.Hname == changedSchedule.Params
                                                                                    .HEATER);

                                                                        var cItem = new Schedule(
                                                                            changedSchedule.Params.SNAME,
                                                                            Circuit<IntelliCenterConnection>.CircuitType
                                                                                .SCHED,
                                                                            changedSchedule, schedCir,
                                                                            changedSchedule.objnam,
                                                                            DataInterface)
                                                                        {
                                                                            SelectedHeater = selectedHeater,
                                                                            Bodies = Bodies.ToList()
                                                                        };


                                                                        StatusMessage = "Item Changed";
                                                                        Device.BeginInvokeOnMainThread(
                                                                            () =>
                                                                            {
                                                                                Schedules.InsertInPlace(cItem,
                                                                                    o => o.ListOrd);
                                                                            });

                                                                        if (Today(changedSchedule.Params.DAY))
                                                                        {
                                                                            Device.BeginInvokeOnMainThread(
                                                                                () =>
                                                                                {
                                                                                    TodaysSchedule.InsertInPlace(cItem,
                                                                                        o => o.ListOrd);
                                                                                });

                                                                        }
                                                                    }
                                                                }

                                                            }
                                                        }
                                                        else if (WriteParamListFields.TryGetValue("objectList[0].changes[0].objnam",
                                                            out var changedObjectName))
                                                        {
                                                            if (HardwareDictionary.TryGetValue(changedObjectName.ToString(),
                                                                out var circuit))
                                                            {
                                                                switch (circuit.CircuitDescription)
                                                                {
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .BODY:
                                                                        Debug.WriteLine("Body Changed");
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .CIRCUIT:
                                                                        Debug.WriteLine("Circuit Changed");
                                                                        //circuit.UpdateItemAsync((JObject) changesObject);
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .HEATER:
                                                                        Debug.WriteLine("Heater Changed");
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .REMOTE:
                                                                        Debug.WriteLine("Remote Changed");
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .CHEM:
                                                                        Debug.WriteLine("Chem Changed");
                                                                        //var c = (Chem)circuit;
                                                                        //c.UpdateItemAsync((JObject) changesObject);
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .CIRCGRP:
                                                                        Debug.WriteLine("Circuit Group Changed");
                                                                        //circuit.UpdateItemAsync((JObject) changesObject);
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .GENERIC:
                                                                        Debug.WriteLine("Generic Changed");
                                                                        //circuit.UpdateItemAsync((JObject) changesObject);
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .PUMP:
                                                                        Debug.WriteLine("Pump Changed");
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .SENSE:
                                                                        Debug.WriteLine("Sense Changed");
                                                                        break;
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .DIMMER:
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .GLOW:
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .GLOWT:
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .INTELLI:
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .LIGHT:
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .MAGIC2:
                                                                    case Circuit<IntelliCenterConnection>.CircuitType
                                                                        .CLRCASC:
                                                                        Debug.WriteLine("Light Changed");
                                                                        break;
                                                                    default:
                                                                        Debug.WriteLine("Not Defined Changed");
                                                                        break;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Debug.WriteLine("Not Defined Changed");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Debug.WriteLine("Not Defined Changed");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                                
                                break;
                            default:
                                var json = JToken.Parse(e);
                                var fieldsCollector = new JsonFieldsCollector(json);
                                var fields = (Dictionary<string,JValue>)fieldsCollector.GetAllFields();
                                if (fields != null)
                                {
                                    if (fields.TryGetValue("response", out var responseValue))
                                    {
                                        if (responseValue.Value.ToString().Contains("404"))
                                        {
                                            StatusMessage = "Request Not Accepted";
                                        }
                                    }
                                }
                                
                                break;
                        }
                    }
                    catch (Exception messageEx)
                    {
                        this._logService.LogError(messageEx.ToString());
                        this._cloudLogService.LogError(messageEx);
                    }
                }
            }
        }

        private async Task ExecuteLoadHardwareDefinitionCommand()
        {
            await DataInterface.CreateConnectionAsync();
        }

        private async Task ExecuteSubscribeDataCommand()
        {
            IsBusy = true;
            StatusMessage = "Subscribing To Data";
            foreach (var kvp in HardwareDictionary)
            {
                switch (kvp.Value.CircuitDescription)
                {
                    case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "BODY");
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CHEM");
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCGRP");
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCUIT");
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "PUMP");
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "SENSE");
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                    case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                    case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                    case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                    case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                    case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                    case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCUIT");
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname,
                            kvp.Value.CircuitDescription.ToString());
                        break;
                    case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                        //DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "HEATER");
                        break;
                }
            }

            await DataInterface.GetScheduleDataAsync();

            IsBusy = false;
        }

        private async void PopulateModels()
        {
            Device.BeginInvokeOnMainThread(
                () =>
                { 
                    Circuits.Clear();
                    CircuitGroup.Clear();
                    Pumps.Clear();
                    Bodies.Clear();
                    Chems.Clear();
                    Lights.Clear();
                    Heaters.Clear();
                    ScheduleHeaters.Clear();
                    AvailableCircuits.Clear();
                    ChemInstalled = Chems.Any();
                    HasCircuits = Circuits.Any();
                    HasCircuitGroups = CircuitGroup.Any();

                    foreach (var kvp in HardwareDictionary)
                    {
                        switch (kvp.Value.CircuitDescription)
                        {
                            case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                                //Bodies.Add(kvp.Value);
                                if (kvp.Value.Display) Bodies.InsertInPlace(kvp.Value, o => o.ListOrd);
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                                Chems.Add(kvp.Value);
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                                //CircuitGroup.Add(kvp.Value);
                                if (kvp.Value.Display) CircuitGroup.InsertInPlace(kvp.Value, o => o.ListOrd);
                                AvailableCircuits.InsertInPlace(kvp.Value, o => o.Name);
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                                //Circuits.Add(kvp.Value);
                                if (kvp.Value.Display) Circuits.InsertInPlace(kvp.Value, o => o.ListOrd);
                                AvailableCircuits.InsertInPlace(kvp.Value, o => o.Name);
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                                //Pumps.Add(kvp.Value);
                                Pumps.InsertInPlace(kvp.Value, o => o.Hname);
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                                var sen = (Sense)kvp.Value;
                                switch (sen.Type)
                                {
                                    case Sense.SenseType.AIR:
                                        AirSensor = kvp.Value;
                                        break;
                                    case Sense.SenseType.SOLAR:
                                        SolarSensor = kvp.Value;
                                        break;
                                    case Sense.SenseType.POOL:
                                        WaterSensor = kvp.Value;
                                        break;
                                }
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                            case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                            case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                            case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                            case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                            case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                            case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                                //Lights.Add(kvp.Value);
                                if (kvp.Value.Display) Circuits.InsertInPlace(kvp.Value, o => o.ListOrd);
                                Lights.InsertInPlace(kvp.Value, o => o.ListOrd);
                                AvailableCircuits.InsertInPlace(kvp.Value, o => o.Name);
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                                var htr = (Heater)kvp.Value;
                                if (htr != null)
                                {
                                    Heaters.Add(htr);
                                }
                                break;
                            case Circuit<IntelliCenterConnection>.CircuitType.POOL:
                            case Circuit<IntelliCenterConnection>.CircuitType.SPA:
                                AvailableCircuits.InsertInPlace(kvp.Value, o => o.Name);
                                break;
                        }

                    }

                    ChemInstalled = Chems.Any();
                    HasCircuits = Circuits.Any();
                    HasCircuitGroups = CircuitGroup.Any();

                    foreach (var bodyCircuit in Bodies)
                    {
                        var body = (Body)bodyCircuit;
                        if (body != null)
                        {
                            body.Heaters.Clear();
                            body.Heaters.Add(new Heater("Heat Off", Heater.HeaterType.GENERIC, "00000", DataInterface));

                            foreach (var heater in Heaters)
                            {
                                if (heater.Bodies.Contains(body.Hname))
                                {
                                    body.Heaters.Add(heater);
                                }
                            }
                        }
                    }

                    if (Heaters.Any())
                    {
                        ScheduleHeaters.Add(new Heater("Off", Heater.HeaterType.GENERIC, "00000", DataInterface));
                        ScheduleHeaters.Add(new Heater("Don't Change", Heater.HeaterType.GENERIC, "00001", DataInterface));

                        foreach (var heater in Heaters)
                        {
                            ScheduleHeaters.Add(heater);
                        }
                    }
                });

            ExecuteSubscribeDataCommand();
        }

        private async Task ExecuteClosingCommand()
        {
            await DataInterface.UnSubscribeAllItemsUpdate();
        }

        private void LoadModels()
        {
            DataInterface.UnSubscribeAllItemsUpdate();
            HardwareDictionary.Clear();
           

            foreach (var obj in HardwareDefinition.answer.SelectMany(answer => answer.Params.Objlist))
            {
                if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(obj.Params.Objtyp, out var circuitType))
                {
                    switch (circuitType)
                    {
                        case Circuit<IntelliCenterConnection>.CircuitType.MODULE:
                            foreach (var moduleCircuit in obj.Params.Circuits)
                            {
                                if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(moduleCircuit.Params.Objtyp,
                                    out var objType))
                                {
                                    switch (objType)
                                    {
                                        case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                                            if (Enum.TryParse<Body.BodyType>(moduleCircuit.Params.Subtyp,
                                                out var bodyType))
                                            {
                                                int.TryParse(moduleCircuit.Params.Listord,
                                                    out var listOrder);
                                                var b = new Body(moduleCircuit.Params.Sname, bodyType, moduleCircuit.Objnam, DataInterface)
                                                {
                                                    ListOrd = listOrder,
                                                    Display = true
                                                };

                                                HardwareDictionary[b.Hname] = b;

                                                if (moduleCircuit.Params.Objlist != null)
                                                {
                                                    foreach (var bodyParam in moduleCircuit.Params.Objlist)
                                                    {
                                                        if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(
                                                            bodyParam.Params.Objtyp,
                                                            out var cktType))
                                                        {
                                                            switch (cktType)
                                                            {
                                                                case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                                                                    if (Enum.TryParse<Chem.ChemType>(
                                                                        bodyParam.Params.Subtyp, out var chemType))
                                                                    {
                                                                        if (chemType == Chem.ChemType.ICHLOR)
                                                                        {
                                                                            var c = new Chem(bodyParam.Params.Sname,
                                                                                chemType, bodyParam.Objnam,
                                                                                DataInterface);

                                                                            switch (bodyType)
                                                                            {
                                                                                case Body.BodyType.POOL:
                                                                                    c.PrimaryName = b.Name;
                                                                                    break;
                                                                                case Body.BodyType.SPA:
                                                                                    c.SecondaryName = b.Name;
                                                                                    break;
                                                                            }


                                                                            HardwareDictionary[c.Hname] = c;
                                                                        }
                                                                    }

                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            

                                            break;
                                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCUIT:
                                            {
                                                if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(moduleCircuit.Params.Subtyp,
                                                    out var subType))
                                                {
                                                    switch (subType)
                                                    {
                                                        case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                                                            if (Enum.TryParse<Light.LightType>(moduleCircuit.Params.Subtyp,
                                                                  out var lightType))
                                                            {
                                                                int.TryParse(moduleCircuit.Params.Listord,
                                                                    out var llistOrder);
                                                                var l = new Light(moduleCircuit.Params.Sname, lightType,
                                                                    moduleCircuit.Objnam, DataInterface)
                                                                {
                                                                    ListOrd = llistOrder,
                                                                    Display = moduleCircuit.Params.Featr == "ON"
                                                                };

                                                                HardwareDictionary[l.Hname] = l;
                                                            }
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                                                            int.TryParse(moduleCircuit.Params.Listord,
                                                                out var glistOrder);
                                                            var gc = new Circuit<IntelliCenterConnection>(moduleCircuit.Params.Sname, subType,
                                                                moduleCircuit.Objnam, DataInterface)
                                                            {
                                                                ListOrd = glistOrder,
                                                                Display = moduleCircuit.Params.Featr == "ON"
                                                            };

                                                            HardwareDictionary[gc.Hname] = gc;
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.POOL:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.SPA:
                                                            int.TryParse(moduleCircuit.Params.Listord,
                                                                    out var blistOrder);
                                                            var b = new Circuit<IntelliCenterConnection>(moduleCircuit.Params.Sname, subType,
                                                                    moduleCircuit.Objnam, DataInterface)
                                                            {
                                                                ListOrd = blistOrder,
                                                                Display = false
                                                            };
                                                            HardwareDictionary[b.Hname] = b;
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                        case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                                            if (Enum.TryParse<Heater.HeaterType>(moduleCircuit.Params.Subtyp,
                                                out var htrType))
                                            {
                                                int.TryParse(moduleCircuit.Params.Listord,
                                                    out var hlistOrder);
                                                var bodies = moduleCircuit.Params.Body.Split(' ');
                                                var h = new Heater(moduleCircuit.Params.Sname, htrType,
                                                    moduleCircuit.Objnam, DataInterface)
                                                {
                                                    Bodies = bodies.ToList(),
                                                    ListOrd = hlistOrder
                                                };

                                                HardwareDictionary[h.Hname] = h;
                                            }
                                            break;
                                    }
                                }
                            }

                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                            {
                                if (Enum.TryParse<Sense.SenseType>(obj.Params.Subtyp, out var senseType))
                                {
                                    var temp = int.Parse(obj.Params.Probe);

                                    var c = new Sense(obj.Params.Sname, senseType)
                                    {
                                        Temp = temp,
                                        Hname = obj.Objnam
                                    };

                                    HardwareDictionary[c.Hname] = c;
                                }
                            }
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCUIT:
                            {
                                if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(obj.Params.Subtyp,
                                    out var subType))
                                {
                                    switch (subType)
                                    {
                                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                                        case Circuit<IntelliCenterConnection>.CircuitType.SPILL:
                                            if (!obj.Params.Featr.Contains("FEATR"))
                                            {
                                                int.TryParse(obj.Params.Listord,
                                                    out var listOrder);

                                                if (obj.Params.Objlist != null)
                                                {

                                                }

                                                var c = new Circuit<IntelliCenterConnection>(obj.Params.Sname, subType,
                                                    obj.Params.Hname, DataInterface)
                                                {
                                                    ListOrd = listOrder,
                                                    Display = obj.Params.Featr == "ON" ||
                                                              subType == Circuit<IntelliCenterConnection>.CircuitType
                                                                  .CIRCGRP
                                                };

                                                HardwareDictionary[c.Hname] = c;
                                            }

                                            break;
                                    }
                                }

                            }
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                            {
                                if (Enum.TryParse<Pump.PumpType>(obj.Params.Subtyp,
                                    out _))
                                {
                                    var p = new Pump(obj.Params.Sname, obj.Objnam)
                                    {
                                        RPM = "-",
                                        GPM = "-",
                                        Power = "-"
                                    };

                                    HardwareDictionary[p.Hname] = p;
                                }
                            }
                            break;
                    }

                }
            }

        }

        private bool Today(string days)
        {
            var date = DateTime.Now;
            bool isToday;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    isToday = days.Contains('F');
                    break;
                case DayOfWeek.Monday:
                    isToday = days.Contains('M');
                    break;
                case DayOfWeek.Saturday:
                    isToday = days.Contains('A');
                    break;
                case DayOfWeek.Sunday:
                    isToday = days.Contains('U');
                    break;
                case DayOfWeek.Thursday:
                    isToday = days.Contains('R');
                    break;
                case DayOfWeek.Tuesday:
                    isToday = days.Contains('T');
                    break;
                case DayOfWeek.Wednesday:
                    isToday = days.Contains('W');
                    break;
                default:
                    isToday = false;
                    break;
            }

            return isToday;
        }


    }
}
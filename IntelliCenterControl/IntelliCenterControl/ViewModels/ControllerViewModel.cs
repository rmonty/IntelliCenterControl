using System;
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

namespace IntelliCenterControl.ViewModels
{
    public class ControllerViewModel : BaseViewModel<IntelliCenterConnection>
    {
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

        public ObservableCollection<Circuit<IntelliCenterConnection>> Circuits { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> CircuitGroup { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Pumps { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Bodies { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Chems { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Lights { get; private set; }
        public ObservableCollection<Heater> Heaters { get; private set; }
        public ObservableCollection<Circuit<IntelliCenterConnection>> Schedules { get; private set; }
        public ConcurrentDictionary<string, Circuit<IntelliCenterConnection>> HardwareDictionary = new ConcurrentDictionary<string, Circuit<IntelliCenterConnection>>();

        private string _airTemp = "-";
        public string AirTemp
        {
            get => _airTemp;
            set => SetProperty(ref _airTemp, value);
        }

        private string _waterTemp = "-";
        public string WaterTemp
        {
            get => _waterTemp;
            set => SetProperty(ref _waterTemp, value);
        }

        private string _saltLevel = "-";

        public string SaltLevel
        {
            get => _saltLevel;
            set => SetProperty(ref _saltLevel, value);
        }

        private bool _chemInstalled;

        public bool ChemInstalled
        {
            get => _chemInstalled;
            set => SetProperty(ref _chemInstalled, value);

        }

        private string _bodySelected;

        public string BodySelected
        {
            get => _bodySelected;
            set => SetProperty(ref _bodySelected, value);
        }


        private DateTime _currentDateTime = new DateTime();

        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value);
        }

        private bool _isConnected;
        private Timer _dateTimeTimer;



        public ControllerViewModel()
        {
            Title = "Pool Control";
            Circuits = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            CircuitGroup = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Pumps = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Bodies = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Chems = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Lights = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            Heaters = new ObservableCollection<Heater>();
            Schedules = new ObservableCollection<Circuit<IntelliCenterConnection>>();
            LoadHardwareDefinitionCommand = new Command(async () => await ExecuteLoadHardwareDefinitionCommand());
            ClosingCommand = new Command(async () => await ExecuteClosingCommand());
            SubscribeDataCommand = new Command(async () => await ExecuteSubscribeDataCommand());
            AllLightsOnCommand = new Command(async () => await ExecuteAllLightsOnCommand());
            AllLightsOffCommand = new Command(async () => await ExecuteAllLightsOffCommand());
            DataInterface.DataReceived += DataStoreDataReceived;
            DataInterface.ConnectionChanged += DataInterface_ConnectionChanged;
            _dateTimeTimer = new Timer(DateTimeTimerElapsed, this, 0, 1000);

        }

        private void DataInterface_ConnectionChanged(object sender, IntelliCenterConnection e)
        {
            if (e.State == IntelliCenterConnection.ConnectionState.Connected)
            {
                _isConnected = true;
                try
                {
                    DataInterface.UnSubscribeAllItemsUpdate();
                    DataInterface.GetItemsDefinitionAsync(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                _isConnected = false;
                Circuits.Clear();
                CircuitGroup.Clear();
                Pumps.Clear();
                Bodies.Clear();
                Chems.Clear();
                Lights.Clear();
                Heaters.Clear();
            }
        }

        public async Task UpdateIPAddress()
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
            if (!String.IsNullOrEmpty(e))
            {
                var data = JsonConvert.DeserializeObject(e);
                var jData = (JObject)(data);

                if (jData.TryGetValue("command", out var commandValue))
                {
                    semaphoreSlim.Wait(DataInterface.Cts.Token);
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

                                                        //Console.WriteLine(e);
                                                        HardwareDefinition =
                                                            JsonConvert.DeserializeObject<HardwareDefinition>(e);

                                                        LoadModels();
                                                        PopulateModels();
                                                        ExecuteSubscribeDataCommand();

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
                                                            if (notifyList.TryGetValue("params", out var pumpValues))
                                                            {
                                                                var pv = (JObject)pumpValues;
                                                                if (pv.TryGetValue("RPM", out var rpm))
                                                                {
                                                                    pump.RPM = rpm.ToString() == "0" ? "-" : rpm.ToString();
                                                                }

                                                                if (pv.TryGetValue("GPM", out var flow))
                                                                {
                                                                    pump.GPM = flow.ToString() == "0" ? "-" : flow.ToString();
                                                                }

                                                                if (pv.TryGetValue("PWR", out var power))
                                                                {
                                                                    pump.Power = power.ToString() == "0"
                                                                        ? "-"
                                                                        : power.ToString();
                                                                }

                                                                if (pv.TryGetValue("STATUS", out var status))
                                                                {
                                                                    var ps = (int)status;
                                                                    if (Enum.IsDefined(typeof(Pump.PumpStatus), ps))
                                                                        pump.Status = (Pump.PumpStatus)ps;
                                                                    else
                                                                        pump.Status = Pump.PumpStatus.OFF;
                                                                }

                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                                                            if (notifyList.TryGetValue("params", out var bodyValues))
                                                            {
                                                                //Console.WriteLine(bodyValues);
                                                                var bv = (JObject)bodyValues;
                                                                var body = (Body)circuit;
                                                                if (bv.TryGetValue("TEMP", out var temp))
                                                                {
                                                                    if (int.TryParse(temp.ToString(), out var bTemp))
                                                                    {
                                                                        body.Temp = bTemp;
                                                                    }

                                                                }

                                                                if (bv.TryGetValue("LSTTMP", out var lsttemp))
                                                                {
                                                                    body.LastTemp = lsttemp.ToString() == "0"
                                                                        ? "-"
                                                                        : lsttemp.ToString();
                                                                    ;
                                                                }

                                                                if (bv.TryGetValue("STATUS", out var status))
                                                                {
                                                                    body.Active = status.ToString() == "ON";
                                                                    if (body.Active)
                                                                    {
                                                                        WaterTemp = body.Temp == 0 ? "-" : body.Temp.ToString();
                                                                    }
                                                                }

                                                                if (bv.TryGetValue("HTMODE", out var htmode))
                                                                {
                                                                    var hm = (int)htmode;
                                                                    if (Enum.IsDefined(typeof(Body.HeatModes), hm))
                                                                    {
                                                                        body.HeatMode = (Body.HeatModes)hm;
                                                                    }
                                                                    else
                                                                    {
                                                                        body.HeatMode = Body.HeatModes.Off;
                                                                    }

                                                                }

                                                                if (bv.TryGetValue("HTSRC", out var htSource))
                                                                {
                                                                    var selectedHeater =
                                                                        body.Heaters.FirstOrDefault<Heater>(h =>
                                                                            h.Hname == htSource.ToString());

                                                                    if (selectedHeater != null)
                                                                    {
                                                                        body.SelectedHeater =
                                                                            body.Heaters.Contains(selectedHeater)
                                                                                ? body.Heaters.IndexOf(selectedHeater)
                                                                                : 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        body.SelectedHeater = 0;
                                                                    }
                                                                }

                                                                if (bv.TryGetValue("LOTMP", out var lTemp))
                                                                {
                                                                    body.LOTemp = lTemp.ToString();
                                                                }
                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                                                            if (notifyList.TryGetValue("params", out var senseValues))
                                                            {
                                                                var sv = (JObject)senseValues;
                                                                var sensor = (Sense)circuit;
                                                                if (sv.TryGetValue("PROBE", out var temp))
                                                                {
                                                                    switch (sensor.Type)
                                                                    {
                                                                        case Sense.SenseType.AIR:
                                                                            AirTemp = temp.ToString() == "0"
                                                                                ? "-"
                                                                                : temp.ToString();
                                                                            break;
                                                                        case Sense.SenseType.POOL:
                                                                            WaterTemp = temp.ToString() == "0"
                                                                                ? "-"
                                                                                : temp.ToString();
                                                                            break;
                                                                        case Sense.SenseType.SOLAR:
                                                                            break;
                                                                        default: break;
                                                                    }
                                                                }
                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                                                            if (notifyList.TryGetValue("params", out var circuitValues))
                                                            {
                                                                var cv = (JObject)circuitValues;
                                                                if (cv.TryGetValue("STATUS", out var stat))
                                                                {
                                                                    switch (stat.ToString())
                                                                    {
                                                                        case "ON":
                                                                            circuit.Active = true;
                                                                            break;
                                                                        case "OFF":
                                                                            circuit.Active = false;
                                                                            break;
                                                                        default: break;
                                                                    }
                                                                }
                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                                                            if (notifyList.TryGetValue("params", out var groupValues))
                                                            {
                                                                var gv = (JObject)groupValues;
                                                                if (gv.TryGetValue("STATUS", out var stat))
                                                                {
                                                                    switch (stat.ToString())
                                                                    {
                                                                        case "ON":
                                                                            circuit.Active = true;
                                                                            break;
                                                                        case "OFF":
                                                                            circuit.Active = false;
                                                                            break;
                                                                        default: break;
                                                                    }
                                                                }
                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                                                            if (notifyList.TryGetValue("params", out var chemValues))
                                                            {
                                                                var cv = (JObject)chemValues;
                                                                var chem = (Chem)circuit;
                                                                if (cv.TryGetValue("SALT", out var salt))
                                                                {
                                                                    ChemInstalled = true;
                                                                    chem.Salt = salt.ToString() == "0" ? "-" : salt.ToString();
                                                                    SaltLevel = chem.Salt;
                                                                }
                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                                                        case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                                                            if (notifyList.TryGetValue("params", out var lightValues))
                                                            {
                                                                var lv = (JObject)lightValues;
                                                                var light = (Light)circuit;
                                                                if (lv.TryGetValue("STATUS", out var stat))
                                                                {
                                                                    switch (stat.ToString())
                                                                    {
                                                                        case "ON":
                                                                            circuit.Active = true;
                                                                            break;
                                                                        case "OFF":
                                                                            circuit.Active = false;
                                                                            break;
                                                                        default: break;
                                                                    }
                                                                }

                                                                if (lv.TryGetValue("USE", out var lightColor))
                                                                {
                                                                    if (Enum.TryParse<Light.LightColors>(lightColor.ToString(),
                                                                        out var color))
                                                                    {
                                                                        light.Color = color;
                                                                    }
                                                                }
                                                            }

                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                                                            if (notifyList.TryGetValue("params", out var heaterValues))
                                                            {
                                                                //Console.WriteLine(heaterValues);
                                                                //var hv = (JObject)heaterValues;
                                                                //var heaterCircuit = (Heater)circuit;
                                                                //if (hv.TryGetValue("BODY", out var bodies))
                                                                //{

                                                                //}
                                                            }

                                                            break;
                                                        default:
                                                            //Console.WriteLine(notifyList);
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

                                                                        Schedules.Clear();
                                                                        ScheduleDefinition =
                                                                            JsonConvert.DeserializeObject<SchedulesDefinition>(e);
                                                                        foreach (var sch in ScheduleDefinition.objectList)
                                                                        {
                                                                            var date = DateTime.Now;
                                                                            var days = sch.Params.DAY;
                                                                            var isToday = date.DayOfWeek switch
                                                                            {
                                                                                DayOfWeek.Monday => days.Contains('M'),
                                                                                DayOfWeek.Friday => days.Contains('F'),
                                                                                DayOfWeek.Saturday => days.Contains('A'),
                                                                                DayOfWeek.Sunday => days.Contains('U'),
                                                                                DayOfWeek.Thursday => days.Contains('R'),
                                                                                DayOfWeek.Tuesday => days.Contains('T'),
                                                                                DayOfWeek.Wednesday => days.Contains('W'),
                                                                                _ => false,
                                                                            };
                                                                            if (isToday)
                                                                            {
                                                                                var startTime = sch.Params.TIME.Split(',');
                                                                                var endTime = sch.Params.TIMOUT.Split(',');
                                                                                if (startTime.Length > 2 && endTime.Length > 2)
                                                                                {

                                                                                    var start = new DateTime(date.Year, date.Month, date.Day,
                                                                                        int.Parse(startTime[0]),
                                                                                        int.Parse(startTime[1]), int.Parse(startTime[2]));
                                                                                    var end = new DateTime(date.Year, date.Month, date.Day,
                                                                                        int.Parse(endTime[0]),
                                                                                        int.Parse(endTime[1]), int.Parse(endTime[2]));

                                                                                    var s = new Schedule(sch.Params.SNAME, Circuit<IntelliCenterConnection>.CircuitType.SCHED,
                                                                                        sch.objnam,
                                                                                        DataInterface)
                                                                                    {
                                                                                        Active = sch.Params.ACT == "ON",
                                                                                        StartTime = start,
                                                                                        EndTime = end
                                                                                    };

                                                                                    Device.BeginInvokeOnMainThread(() =>
                                                                                    {
                                                                                        Schedules.Add(s);
                                                                                    });
                                                                                    HardwareDictionary[s.Hname] = s;
                                                                                }
                                                                            }
                                                                        }


                                                                        return;
                                                                    default:
                                                                        break;
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
                            default:
                                //Console.WriteLine(e);
                                break;
                        }
                    }
                    catch (Exception messageEx)
                    {
                        Console.WriteLine(messageEx);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
            }
        }

        private async Task ExecuteLoadHardwareDefinitionCommand()
        {
            try
            {
                await DataInterface.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private async Task ExecuteSubscribeDataCommand()
        {
            if (!_isConnected)
            {
                await DataInterface.CreateConnectionAsync();
            }

            IsBusy = true;
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
                    default:
                        break;
                }
            }

            await DataInterface.GetScheduleDataAsync();

            IsBusy = false;
        }

        private void PopulateModels()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Circuits.Clear();
                CircuitGroup.Clear();
                Pumps.Clear();
                Bodies.Clear();
                Chems.Clear();
                Lights.Clear();
                Heaters.Clear();

                foreach (var kvp in HardwareDictionary)
                {
                    switch (kvp.Value.CircuitDescription)
                    {
                        case Circuit<IntelliCenterConnection>.CircuitType.BODY:
                            //Bodies.Add(kvp.Value);
                            Bodies.InsertInPlace(kvp.Value, o => o.ListOrd);
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                            Chems.Add(kvp.Value);
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                            //CircuitGroup.Add(kvp.Value);
                            CircuitGroup.InsertInPlace(kvp.Value, o => o.ListOrd);
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                            //Circuits.Add(kvp.Value);
                            Circuits.InsertInPlace(kvp.Value, o => o.ListOrd);
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                            //Pumps.Add(kvp.Value);
                            Pumps.InsertInPlace(kvp.Value, o => o.Hname);
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.SENSE:
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.INTELLI:
                        case Circuit<IntelliCenterConnection>.CircuitType.GLOW:
                        case Circuit<IntelliCenterConnection>.CircuitType.MAGIC2:
                        case Circuit<IntelliCenterConnection>.CircuitType.CLRCASC:
                        case Circuit<IntelliCenterConnection>.CircuitType.DIMMER:
                        case Circuit<IntelliCenterConnection>.CircuitType.GLOWT:
                        case Circuit<IntelliCenterConnection>.CircuitType.LIGHT:
                            //Lights.Add(kvp.Value);
                            Lights.InsertInPlace(kvp.Value, o => o.ListOrd);
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                            var htr = (Heater) kvp.Value;
                            if (htr != null)
                            {
                                Heaters.Add(htr);
                            }

                            break;
                        default:
                            break;
                    }

                }

                foreach (var bodyCircuit in Bodies)
                {
                    var body = (Body) bodyCircuit;
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
            });
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
                                                    LastTemp = "-",
                                                    ListOrd = listOrder
                                                };

                                                HardwareDictionary[b.Hname] = b;
                                            }

                                            foreach (var bodyParam in moduleCircuit.Params.Objlist)
                                            {
                                                if (Enum.TryParse<Circuit<IntelliCenterConnection>.CircuitType>(
                                                    bodyParam.Params.Objtyp.ToString(),
                                                    out var cktType))
                                                {
                                                    switch (cktType)
                                                    {
                                                        case Circuit<IntelliCenterConnection>.CircuitType.CHEM:
                                                            if (Enum.TryParse<Chem.ChemType>(
                                                                bodyParam.Params.Subtyp, out var chemType))
                                                            {
                                                                var c = new Chem(bodyParam.Params.Sname, chemType)
                                                                {
                                                                    Hname = bodyParam.Objnam
                                                                };

                                                                HardwareDictionary[c.Hname] = c;
                                                            }

                                                            break;
                                                        default: break;
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
                                                                    out var listOrder);
                                                                var l = new Light(moduleCircuit.Params.Sname, lightType,
                                                                    moduleCircuit.Objnam, DataInterface)
                                                                {
                                                                    ListOrd = listOrder
                                                                };

                                                                HardwareDictionary[l.Hname] = l;
                                                            }
                                                            break;
                                                        case Circuit<IntelliCenterConnection>.CircuitType.GENERIC:
                                                            if (moduleCircuit.Params.Featr == "ON")
                                                            {
                                                                int.TryParse(moduleCircuit.Params.Listord,
                                                                    out var listOrder);
                                                                var gc = new Circuit<IntelliCenterConnection>(moduleCircuit.Params.Sname, Circuit<IntelliCenterConnection>.CircuitType.GENERIC,
                                                                    moduleCircuit.Objnam, DataInterface)
                                                                {
                                                                    ListOrd = listOrder
                                                                };

                                                                HardwareDictionary[gc.Hname] = gc;
                                                            }
                                                            break;
                                                        default: break;
                                                    }
                                                }
                                            }
                                            break;
                                        case Circuit<IntelliCenterConnection>.CircuitType.HEATER:
                                            if (Enum.TryParse<Heater.HeaterType>(moduleCircuit.Params.Subtyp,
                                                out var htrType))
                                            {
                                                var bodies = moduleCircuit.Params.Body.Split(" ");
                                                var h = new Heater(moduleCircuit.Params.Sname, htrType,
                                                    moduleCircuit.Objnam, DataInterface)
                                                {
                                                    Bodies = bodies.ToList()
                                                };

                                                HardwareDictionary[h.Hname] = h;
                                            }
                                            break;
                                        default:
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
                                            {
                                                if (obj.Params.Featr == "ON")
                                                {
                                                    int.TryParse(obj.Params.Listord,
                                                        out var listOrder);
                                                    var c = new Circuit<IntelliCenterConnection>(obj.Params.Sname, Circuit<IntelliCenterConnection>.CircuitType.GENERIC,
                                                        obj.Params.Hname, DataInterface)
                                                    {
                                                        ListOrd = listOrder
                                                    };

                                                    HardwareDictionary[c.Hname] = c;
                                                }
                                            }
                                            break;
                                        case Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP:
                                            {
                                                int.TryParse(obj.Params.Listord,
                                                    out var listOrder);
                                                var c = new Circuit<IntelliCenterConnection>(obj.Params.Sname, Circuit<IntelliCenterConnection>.CircuitType.CIRCGRP,
                                                    obj.Params.Hname, DataInterface)
                                                {
                                                    ListOrd = listOrder
                                                };
                                                HardwareDictionary[c.Hname] = c;
                                            }
                                            break;
                                        default: break;
                                    }
                                }

                            }
                            break;
                        case Circuit<IntelliCenterConnection>.CircuitType.PUMP:
                            {
                                if (Enum.TryParse<Pump.PumpType>(obj.Params.Subtyp,
                                    out var subType))
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
                        default:
                            break;
                    }

                }
            }

        }


    }
}
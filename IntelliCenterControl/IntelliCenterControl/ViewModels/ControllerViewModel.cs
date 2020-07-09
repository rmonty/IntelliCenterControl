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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace IntelliCenterControl.ViewModels
{
    public class ControllerViewModel : BaseViewModel<HardwareDefinition>
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

        public ObservableCollection<Circuit> Circuits { get; private set; }
        public ObservableCollection<Circuit> CircuitGroup { get; private set; }
        public ObservableCollection<Circuit> Pumps { get; private set; }
        public ObservableCollection<Circuit> Bodies { get; private set; }
        public ObservableCollection<Circuit> Chems { get; private set; }
        public ObservableCollection<Circuit> Lights { get; private set; }
        public ObservableCollection<Circuit> BodyHeaters { get; private set; }
        public ObservableCollection<Heater> Heaters { get; private set; }
        public ObservableCollection<Circuit> Schedules { get; private set; }
        public ConcurrentDictionary<string, Circuit> HardwareDictionary = new ConcurrentDictionary<string, Circuit>();

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


        private double _heatTemp = 60;

        public double HeatTemp
        {
            get => _heatTemp;
            set => SetProperty(ref _heatTemp, value, "HeatTemp", new Action(async () =>
            {
                var element = BodyHeaters.ElementAtOrDefault<Circuit>(SelectedHeater);
                if (element != null)
                {
                    //await DataInterface.SendItemUpdateAsync(BodySelected, "HTSRC", element.Hname);
                }
            }));
        }

        private bool _heatOn;

        public bool HeatOn
        {
            get => _heatOn;
            set => SetProperty(ref _heatOn, value, "HeatOn", new Action(async () =>
            {
                var element = BodyHeaters.ElementAtOrDefault<Circuit>(SelectedHeater);
                if (element != null)
                {
                    //await DataInterface.SendItemUpdateAsync(BodySelected, "HTSRC", element.Hname);
                }
            }));
        }

        private int _selectedHeater;

        public int SelectedHeater
        {
            get => _selectedHeater;
            set => SetProperty(ref _selectedHeater, value, "SelectedHeater", new Action(async () =>
            {
                var element = BodyHeaters.ElementAtOrDefault<Circuit>(value);
                if (element != null)
                {
                    //await DataInterface.SendItemUpdateAsync(BodySelected, "HTSRC", element.Hname);
                }
            }));
        }

        private DateTime _currentDateTime;

        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value);
        }


        private Timer _dateTimeTimer; 

        

        public ControllerViewModel()
        {
            Title = "Pool";
            Circuits = new ObservableCollection<Circuit>();
            CircuitGroup = new ObservableCollection<Circuit>();
            Pumps = new ObservableCollection<Circuit>();
            Bodies = new ObservableCollection<Circuit>();
            Chems = new ObservableCollection<Circuit>();
            Lights = new ObservableCollection<Circuit>();
            BodyHeaters = new ObservableCollection<Circuit>();
            Heaters = new ObservableCollection<Heater>();
            Schedules = new ObservableCollection<Circuit>();
            LoadHardwareDefinitionCommand = new Command(async () => await ExecuteLoadHardwareDefinitionCommand());
            ClosingCommand = new Command(async () => await ExecuteClosingCommand());
            SubscribeDataCommand = new Command(async () => await ExecuteSubscribeDataCommand());
            AllLightsOnCommand = new Command(async () => await ExecuteAllLightsOnCommand());
            AllLightsOffCommand = new Command(async () => await ExecuteAllLightsOffCommand());
            DataInterface.DataReceived += DataStoreDataReceived;
            _dateTimeTimer = new Timer(DateTimeTimerElapsed, this, 0, 1000);

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
            if(cvm != null) cvm.CurrentDateTime = DateTime.Now;
        }

        private async Task ExecuteAllLightsOnCommand()
        {
            await DataInterface.SendItemUpdateAsync("_A111", "STATUS", "ON");
        }

        private async Task ExecuteAllLightsOffCommand()
        {
            await DataInterface.SendItemUpdateAsync("_A110", "STATUS", "OFF");
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
                                                    await semaphoreSlim.WaitAsync();
                                                    try
                                                    {
                                                        //Console.WriteLine(e);
                                                        HardwareDefinition =
                                                            JsonConvert.DeserializeObject<HardwareDefinition>(e);

                                                        await LoadModels();
                                                        ExecuteSubscribeDataCommand();
                                                    }
                                                    finally
                                                    {
                                                        semaphoreSlim.Release();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "NotifyList":
                            if (jData.TryGetValue("objectList", out var objectListValue))
                            {
                                var jDataArray = (JArray)objectListValue;

                                if (jDataArray != null && jDataArray.Count >= 1)
                                {
                                    var notifyList = (JObject)jDataArray[0];

                                    if (notifyList.TryGetValue("objnam", out var objName))
                                    {
                                        if (HardwareDictionary.TryGetValue(objName.ToString(), out var circuit))
                                        {
                                            switch (circuit.CircuitDescription)
                                            {
                                                case Circuit.CircuitType.PUMP:
                                                    var pump = (Pump) circuit;
                                                    if (notifyList.TryGetValue("params", out var pumpValues))
                                                    {
                                                        var pv = (JObject) pumpValues;
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
                                                            var ps = (int) status;
                                                            if (Enum.IsDefined(typeof(Pump.PumpStatus), ps))
                                                                pump.Status = (Pump.PumpStatus) ps;
                                                            else
                                                                pump.Status = Pump.PumpStatus.OFF;
                                                        }
                                                    }

                                                    break;
                                                case Circuit.CircuitType.BODY:
                                                    if (notifyList.TryGetValue("params", out var bodyValues))
                                                    {
                                                        //Console.WriteLine(bodyValues);
                                                        var bv = (JObject) bodyValues;
                                                        var body = (Body) circuit;
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
                                                                BodySelected = body.Hname;
                                                                
                                                            }
                                                        }

                                                        if (bv.TryGetValue("HTMODE", out var htmode))
                                                        {
                                                            var hm = (int) htmode;
                                                            if (Enum.IsDefined(typeof(Body.HeatModes), hm))
                                                            {
                                                                body.HeatMode = (Body.HeatModes) hm;
                                                                
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
                                                                body.SelectedHeater = body.Heaters.Contains(selectedHeater) ? body.Heaters.IndexOf(selectedHeater) : 0;
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
                                                case Circuit.CircuitType.SENSE:
                                                    if (notifyList.TryGetValue("params", out var senseValues))
                                                    {
                                                        var sv = (JObject) senseValues;
                                                        var sensor = (Sense) circuit;
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
                                                case Circuit.CircuitType.GENERIC:
                                                    if (notifyList.TryGetValue("params", out var circuitValues))
                                                    {
                                                        var cv = (JObject) circuitValues;
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
                                                case Circuit.CircuitType.CIRCGRP:
                                                    if (notifyList.TryGetValue("params", out var groupValues))
                                                    {
                                                        var gv = (JObject) groupValues;
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
                                                case Circuit.CircuitType.CHEM:
                                                    if (notifyList.TryGetValue("params", out var chemValues))
                                                    {
                                                        var cv = (JObject) chemValues;
                                                        var chem = (Chem) circuit;
                                                        if (cv.TryGetValue("SALT", out var salt))
                                                        {
                                                            ChemInstalled = true;
                                                            chem.Salt = salt.ToString() == "0" ? "-" : salt.ToString();
                                                            SaltLevel = chem.Salt;
                                                        }
                                                    }

                                                    break;
                                                case Circuit.CircuitType.INTELLI:
                                                case Circuit.CircuitType.GLOW:
                                                case Circuit.CircuitType.MAGIC2:
                                                case Circuit.CircuitType.CLRCASC:
                                                case Circuit.CircuitType.DIMMER:
                                                case Circuit.CircuitType.GLOWT:
                                                case Circuit.CircuitType.LIGHT:
                                                    if (notifyList.TryGetValue("params", out var lightValues))
                                                    {
                                                        var lv = (JObject) lightValues;
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
                                                            if(Enum.TryParse<Light.LightColors>(lightColor.ToString(), out var color))
                                                            {
                                                                light.Color = color;
                                                            }
                                                        }
                                                    }

                                                    break;
                                                case Circuit.CircuitType.HEATER:
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
                                    }
                                }
                            }
                            break;
                        case "SendParamList":
                            await semaphoreSlim.WaitAsync();
                            try
                            {
                                Schedules.Clear();
                                ScheduleDefinition =
                                    JsonConvert.DeserializeObject<SchedulesDefinition>(e);
                                foreach (var sch in ScheduleDefinition.objectList)
                                {
                                    var startTime = sch.Params.TIME.Split(',');
                                    var endTime = sch.Params.TIMOUT.Split(',');
                                    if (startTime.Length > 2 && endTime.Length > 2)
                                    {
                                        var date = DateTime.Now;
                                        var start = new DateTime(date.Year, date.Month, date.Day, int.Parse(startTime[0]),
                                            int.Parse(startTime[1]), int.Parse(startTime[2]));
                                        var end = new DateTime(date.Year, date.Month, date.Day, int.Parse(endTime[0]),
                                            int.Parse(endTime[1]), int.Parse(endTime[2]));

                                        var s = new Schedule(sch.Params.SNAME, Circuit.CircuitType.SCHED, sch.objnam,
                                            DataInterface)
                                        {
                                            Active = sch.Params.ACT == "ON",
                                            StartTime = start,
                                            EndTime = end
                                        };
                                        Schedules.Add(s);
                                        HardwareDictionary[s.Hname] = s;
                                    }
                                }

                            }
                            catch (Exception scheduleEx)
                            {
                                Console.WriteLine(scheduleEx);
                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }

                            break;
                        default:
                            Console.WriteLine(e);
                            break;
                    }
                }
            }
        }

        private async Task ExecuteLoadHardwareDefinitionCommand()
        {
            try
            {
                await DataInterface.GetItemsDefinitionAsync(true);
                //await DataInterface.GetScheduleDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private async Task ExecuteSubscribeDataCommand()
        {
            IsBusy = true;
            await PopulateModels();

            foreach (var kvp in HardwareDictionary)
            {
                switch (kvp.Value.CircuitDescription)
                {
                    case Circuit.CircuitType.BODY:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "BODY");
                        break;
                    case Circuit.CircuitType.CHEM:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CHEM");
                        break;
                    case Circuit.CircuitType.CIRCGRP:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCGRP");
                        break;
                    case Circuit.CircuitType.GENERIC:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCUIT");
                        break;
                    case Circuit.CircuitType.PUMP:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "PUMP");
                        break;
                    case Circuit.CircuitType.SENSE:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "SENSE");
                        break;
                    case Circuit.CircuitType.INTELLI:
                    case Circuit.CircuitType.GLOW:
                    case Circuit.CircuitType.MAGIC2:
                    case Circuit.CircuitType.CLRCASC:
                    case Circuit.CircuitType.DIMMER:
                    case Circuit.CircuitType.GLOWT:
                    case Circuit.CircuitType.LIGHT:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCUIT");
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, kvp.Value.CircuitDescription.ToString());
                        break;
                    case Circuit.CircuitType.HEATER:
                        DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "HEATER");
                        break;
                    default:
                        break;
                }
            }

            await DataInterface.GetScheduleDataAsync();
            IsBusy = false;

        }

        private async Task PopulateModels()
        {
            Circuits.Clear();
            CircuitGroup.Clear();
            Pumps.Clear();
            Bodies.Clear();
            Chems.Clear();
            Lights.Clear();
            BodyHeaters.Clear();
            Heaters.Clear();


            foreach (var kvp in HardwareDictionary)
            {
                switch (kvp.Value.CircuitDescription)
                {
                    case Circuit.CircuitType.BODY:
                        Bodies.InsertInPlace(kvp.Value, o=>o.ListOrd);
                        break;
                    case Circuit.CircuitType.CHEM:
                        Chems.Add(kvp.Value);
                        break;
                    case Circuit.CircuitType.CIRCGRP:
                        CircuitGroup.InsertInPlace(kvp.Value, o => o.ListOrd);
                        break;
                    case Circuit.CircuitType.GENERIC:
                        Circuits.InsertInPlace(kvp.Value, o => o.ListOrd);
                        break;
                    case Circuit.CircuitType.PUMP:
                        Pumps.InsertInPlace(kvp.Value, o => o.Hname);
                        break;
                    case Circuit.CircuitType.SENSE:
                        break;
                    case Circuit.CircuitType.INTELLI:
                    case Circuit.CircuitType.GLOW:
                    case Circuit.CircuitType.MAGIC2:
                    case Circuit.CircuitType.CLRCASC:
                    case Circuit.CircuitType.DIMMER:
                    case Circuit.CircuitType.GLOWT:
                    case Circuit.CircuitType.LIGHT:
                        Lights.InsertInPlace(kvp.Value, o => o.ListOrd);
                        break;
                    case Circuit.CircuitType.HEATER:
                        var htr = (Heater)kvp.Value;
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
                var body = (Body)bodyCircuit;
                if (body != null)
                {
                    body.Heaters.Clear();
                    body.Heaters.Add(new Heater("Heat Off",Heater.HeaterType.GENERIC, "00000", DataInterface));

                    foreach (var heater in Heaters)
                    {
                        if (heater.Bodies.Contains(body.Hname))
                        {
                            body.Heaters.Add(heater);
                        }
                    }
                }
            }
        }

        private async Task ExecuteClosingCommand()
        {
            await DataInterface.UnSubscribeAllItemsUpdate();
        }

        private async Task LoadModels()
        {
            await DataInterface.UnSubscribeAllItemsUpdate();
            HardwareDictionary.Clear();

            foreach (var obj in HardwareDefinition.answer.SelectMany(answer => answer.Params.Objlist))
            {
                if (Enum.TryParse<Circuit.CircuitType>(obj.Params.Objtyp, out var circuitType))
                {
                    switch (circuitType)
                    {
                        case Circuit.CircuitType.MODULE:
                            foreach (var moduleCircuit in obj.Params.Circuits)
                            {
                                if (Enum.TryParse<Circuit.CircuitType>(moduleCircuit.Params.Objtyp,
                                    out var objType))
                                {
                                    switch (objType)
                                    {
                                        case Circuit.CircuitType.BODY:
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
                                                if (Enum.TryParse<Circuit.CircuitType>(
                                                    bodyParam.Params.Objtyp.ToString(),
                                                    out var cktType))
                                                {
                                                    switch (cktType)
                                                    {
                                                        case Circuit.CircuitType.CHEM:
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
                                        case Circuit.CircuitType.CIRCUIT:
                                            {
                                                if (Enum.TryParse<Circuit.CircuitType>(moduleCircuit.Params.Subtyp,
                                                    out var subType))
                                                {
                                                    switch (subType)
                                                    {
                                                        case Circuit.CircuitType.INTELLI:
                                                        case Circuit.CircuitType.GLOW:
                                                        case Circuit.CircuitType.MAGIC2:
                                                        case Circuit.CircuitType.CLRCASC:
                                                        case Circuit.CircuitType.DIMMER:
                                                        case Circuit.CircuitType.GLOWT:
                                                        case Circuit.CircuitType.LIGHT:
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

                                                        default: break;
                                                    }
                                                }
                                            }
                                            break;
                                        case Circuit.CircuitType.HEATER:
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
                        case Circuit.CircuitType.SENSE:
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
                        case Circuit.CircuitType.CIRCUIT:
                            {
                                if (Enum.TryParse<Circuit.CircuitType>(obj.Params.Subtyp,
                                    out var subType))
                                {
                                    switch (subType)
                                    {
                                        case Circuit.CircuitType.GENERIC:
                                            {
                                                if (obj.Params.Featr == "ON")
                                                {
                                                    int.TryParse(obj.Params.Listord,
                                                        out var listOrder);
                                                    var c = new Circuit(obj.Params.Sname, Circuit.CircuitType.GENERIC,
                                                        obj.Params.Hname, DataInterface)
                                                    {
                                                        ListOrd = listOrder
                                                    };

                                                    HardwareDictionary[c.Hname] = c;
                                                }
                                            }
                                            break;
                                        case Circuit.CircuitType.CIRCGRP:
                                            {
                                                int.TryParse(obj.Params.Listord,
                                                    out var listOrder);
                                                var c = new Circuit(obj.Params.Sname, Circuit.CircuitType.CIRCGRP,
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
                        case Circuit.CircuitType.PUMP:
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
using System;
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
    public class HardwareDefinitionViewModel : BaseViewModel<HardwareDefinition>
    {
        private HardwareDefinition _hardwareDefinition = new HardwareDefinition();
        public HardwareDefinition HardwareDefinition
        {
            get => _hardwareDefinition;
            set => SetProperty(ref _hardwareDefinition, value);
        }
        public Command LoadHardwareDefinitionCommand { get; set; }
        public Command ClosingCommand { get; set; }
        public Command SubscribeDataCommand { get; set; }

        public ObservableCollection<Circuit> Circuits { get; private set; }
        public ObservableCollection<Circuit> CircuitGroup { get; private set; }
        public ObservableCollection<Circuit> Pumps { get; private set; }
        public ObservableCollection<Circuit> Bodies { get; private set; }
        public ObservableCollection<Circuit> Chems { get; private set; }
        public Dictionary<string, Circuit> HardwareDictionary = new Dictionary<string, Circuit>();

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
        

        public HardwareDefinitionViewModel()
        {
            Title = "Pool";
            Circuits = new ObservableCollection<Circuit>();
            CircuitGroup = new ObservableCollection<Circuit>();
            Pumps = new ObservableCollection<Circuit>();
            Bodies = new ObservableCollection<Circuit>();
            Chems = new ObservableCollection<Circuit>();
            LoadHardwareDefinitionCommand = new Command(async () => await ExecuteLoadHardwareDefinitionCommand());
            ClosingCommand = new Command(async () => await ExecuteClosingCommand());
            SubscribeDataCommand = new Command(async () => await ExecuteSubscribeDataCommand());
            DataInterface.DataReceived += DataStoreDataReceived;

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
                                                        HardwareDefinition =
                                                            JsonConvert.DeserializeObject<HardwareDefinition>(e);

                                                        await LoadModels();
                                                        await ExecuteSubscribeDataCommand();
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
                                                    var pump = (Pump)circuit;
                                                    //var pump = (Pump)Pumps.FirstOrDefault(c => c.Hname == dictPump.Hname);
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
                                                            pump.Power = power.ToString() == "0" ? "-" : power.ToString();
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
                                                case Circuit.CircuitType.BODY:
                                                    if (notifyList.TryGetValue("params", out var bodyValues))
                                                    {
                                                        var bv = (JObject)bodyValues;
                                                        var body = (Body)circuit;
                                                        if (bv.TryGetValue("TEMP", out var temp))
                                                        {
                                                            WaterTemp = temp.ToString() == "0" ? "-" : temp.ToString();
                                                        }

                                                        if (bv.TryGetValue("LSTTMP", out var lsttemp))
                                                        {
                                                            body.LastTemp = lsttemp.ToString() == "0" ? "-" : lsttemp.ToString(); ;
                                                        }

                                                        if (bv.TryGetValue("STATUS", out var status))
                                                        {
                                                            body.Active = status.ToString() == "ON";
                                                            if (!body.Active)
                                                            {
                                                                body.LastTemp = WaterTemp;
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
                                                    }
                                                    break;
                                                case Circuit.CircuitType.SENSE:
                                                    if (notifyList.TryGetValue("params", out var senseValues))
                                                    {
                                                        var sv = (JObject)senseValues;
                                                        var sensor = (Sense)circuit;
                                                        if (sv.TryGetValue("PROBE", out var temp))
                                                        {

                                                            switch (sensor.Type)
                                                            {
                                                                case Sense.SenseType.AIR:
                                                                    AirTemp = temp.ToString() == "0" ? "-" : temp.ToString();
                                                                    break;
                                                                case Sense.SenseType.POOL:
                                                                    WaterTemp = temp.ToString() == "0" ? "-" : temp.ToString();
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
                                                case Circuit.CircuitType.CIRCGRP:
                                                    if (notifyList.TryGetValue("params", out var groupValues))
                                                    {
                                                        var gv = (JObject)groupValues;
                                                        if (gv.TryGetValue("STATUS", out var stat))
                                                        {
                                                            //var gckt = Circuits.FirstOrDefault(c => c.Hname == circuit.Hname);
                                                            //if (gckt != null)
                                                            //{
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
                                                            //}
                                                        }
                                                    }
                                                    break;
                                                case Circuit.CircuitType.CHEM:
                                                    if (notifyList.TryGetValue("params", out var chemValues))
                                                    {
                                                        var cv = (JObject)chemValues;
                                                        var chem = (Chem)circuit;
                                                        if (cv.TryGetValue("SALT", out var salt))
                                                        {
                                                            chem.Salt = salt.ToString() == "0" ? "-" : salt.ToString();
                                                            SaltLevel = chem.Salt;
                                                        }
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
                        default: break;
                    }
                }
            }
        }

        private async Task ExecuteLoadHardwareDefinitionCommand()
        {
            try
            {
                await DataInterface.GetItemsDefinitionAsync(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private async Task ExecuteSubscribeDataCommand()
        {
            IsBusy = true;
            Circuits.Clear();
            CircuitGroup.Clear();
            Pumps.Clear();
            Bodies.Clear();
            Chems.Clear();
            
            foreach (var kvp in HardwareDictionary)
            {
                switch (kvp.Value.CircuitDescription)
                {
                    case Circuit.CircuitType.BODY:
                        Bodies.Add(kvp.Value);
                        await DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "BODY");
                        break;
                    case Circuit.CircuitType.CHEM:
                        Chems.Add(kvp.Value);
                        await DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CHEM");
                        break;
                    case Circuit.CircuitType.CIRCGRP:
                        CircuitGroup.Add(kvp.Value);
                        await DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCGRP");
                        break;
                    case Circuit.CircuitType.GENERIC:
                        Circuits.Add(kvp.Value);
                        await DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "CIRCUIT");
                        break;
                    case Circuit.CircuitType.PUMP:
                        Pumps.Add(kvp.Value);
                        await DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "PUMP");
                        break;
                    case Circuit.CircuitType.SENSE:
                        await DataInterface.SubscribeItemUpdateAsync(kvp.Value.Hname, "SENSE");
                        break;
                    default:
                        break;
                }
            }
            IsBusy = false;
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
                                                var b = new Body(moduleCircuit.Params.Sname, bodyType)
                                                {
                                                    Hname = moduleCircuit.Objnam,
                                                    LastTemp = "-"
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
                                                //if (Enum.TryParse<Circuit.CircuitType>(moduleCircuit.Params.Subtyp,
                                                //    out var subType))
                                                //{
                                                //    switch (subType)
                                                //    {
                                                //            case Circuit.CircuitType.GENERIC: break;

                                                //            default: break;
                                                //    }
                                                //}
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
                                                    var c = new Circuit(obj.Params.Sname, Circuit.CircuitType.GENERIC,
                                                        obj.Params.Hname, DataInterface);
                                                    HardwareDictionary[c.Hname] = c;
                                                }
                                            }
                                            break;
                                        case Circuit.CircuitType.CIRCGRP:
                                            {
                                                var c = new Circuit(obj.Params.Sname, Circuit.CircuitType.CIRCGRP,
                                                    obj.Params.Hname, DataInterface);
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
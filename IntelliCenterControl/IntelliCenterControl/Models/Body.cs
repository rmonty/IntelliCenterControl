using IntelliCenterControl.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace IntelliCenterControl.Models
{
    public class Body : Circuit<IntelliCenterConnection>
    {
        public const string BodyKeys = "[\"TEMP\",\"STATUS\",\"HTMODE\",\"MODE\",\"LSTTMP\",\"HTSRC\", \"HITMP\", \"LOTMP\"]";

        public Command SendTemperatureCommand {get;set;}

        public enum BodyType
        {
            [Description("Pool")]
            POOL,
            [Description("Spa")]
            SPA
        }

        public enum HeatModes
        {
            Off = 0,
            Flame = 1,
            Solar = 2,
            Flake = 3,
            Ultra = 4,
            Hybrid = 5,
            MasterTemp = 6,
            MaxETemp = 7
        }

        private BodyType _type;

        public BodyType Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        private int _temp;

        public int Temp
        {
            get => _temp;
            set
            {
                if (_temp == value) return;
                _temp = value;
                OnPropertyChanged();
            }
        }

        private string _lastTemp = "-";

        public string LastTemp
        {
            get => _lastTemp;
            set
            {
                if (_lastTemp == value) return;
                _lastTemp = value;
                OnPropertyChanged();
            }
        }

        private HeatModes _heatMode;

        public HeatModes HeatMode
        {
            get => _heatMode;
            set
            {
                if (_heatMode == value) return;
                _heatMode = value;
                OnPropertyChanged();
            }
        }

        private int _selectedHeater;

        public int SelectedHeater
        {
            get => _selectedHeater;
            set
            {
                if (_selectedHeater == value) return;
                _selectedHeater = value;
                OnPropertyChanged();
                Task.Run(ExecuteSelectedHeatSourceCommand);
            }
        }

        private string _loTemp;

        public string LOTemp
        {
            get => _loTemp;
            set
            {
                if (_loTemp == value) return;
                _loTemp = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Heater> _heaters = new ObservableCollection<Heater>();

        public ObservableCollection<Heater> Heaters
        {
            get => _heaters;
            set
            {
                _heaters = value;
                OnPropertyChanged();
            }
        }



        public Body(string name, BodyType bodyType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, CircuitType.BODY, hName, dataInterface)
        {
            Type = bodyType;
            SendTemperatureCommand = new Command(async () => await ExecuteSendTemperatureCommand());
        }

        private async Task ExecuteSendTemperatureCommand()
        {
            if (int.TryParse(_loTemp, out var temp))
            {
                if (DataInterface != null && temp >= 60 && temp <= 104)
                {
                    //await DataInterface.UnSubscribeItemUpdate(Hname);
                    await DataInterface.SendItemParamsUpdateAsync(Hname, "LOTMP", temp.ToString());
                    //await DataInterface.SubscribeItemUpdateAsync(Hname, "BODY");
                }
            }
        }

        protected override async Task ExecuteToggleCircuitCommand()
        {
            if (DataInterface != null)
            {
                var val = Active ? "ON" : "OFF";
                await DataInterface.SendItemCommandUpdateAsync(Hname, "SETBODYSTATE", val);
            }
        }

        private async Task ExecuteSelectedHeatSourceCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                var val = Heaters[SelectedHeater].Hname;
                await DataInterface.SendItemParamsUpdateAsync(Hname, "HEATER", val);
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "BODY");
            }
        }

       

        public override async Task UpdateItemAsync(JObject data)
        {
            if (data.TryGetValue("params", out var bodyValues))
            {
                //Debug.WriteLine(bodyValues);
                var bv = (JObject)bodyValues;

                if (bv.TryGetValue("TEMP", out var temp))
                {
                    if (int.TryParse(temp.ToString(), out var bTemp))
                    {
                        Temp = bTemp;
                    }

                }

                if (bv.TryGetValue("LSTTMP", out var lsttemp))
                {
                    LastTemp = lsttemp.ToString() == "0"
                        ? "-"
                        : lsttemp.ToString();
                }

                if (bv.TryGetValue("STATUS", out var status))
                {
                    UpdateActiveState(status.ToString() == "ON");
                    //if (body.Active)
                    //{
                    //    WaterTemp = body.Temp == 0 ? "-" : body.Temp.ToString();
                    //}
                }

                if (bv.TryGetValue("HTMODE", out var htmode))
                {
                    var hm = (int)htmode;
                    if (Enum.IsDefined(typeof(HeatModes), hm))
                    {
                        HeatMode = (HeatModes)hm;
                    }
                    else
                    {
                        HeatMode = HeatModes.Off;
                    }

                }

                if (bv.TryGetValue("HTSRC", out var htSource))
                {
                    var selectedHeater =
                        Heaters.FirstOrDefault(h =>
                            h.Hname == htSource.ToString());

                    if (selectedHeater != null)
                    {
                        SelectedHeater =
                            Heaters.Contains(selectedHeater)
                                ? Heaters.IndexOf(selectedHeater)
                                : 0;
                    }
                    else
                    {
                        SelectedHeater = 0;
                    }
                }

                if (bv.TryGetValue("LOTMP", out var lTemp))
                {
                    LOTemp = lTemp.ToString();
                }
            }
        }

    }
}

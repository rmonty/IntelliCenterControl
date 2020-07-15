using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using IntelliCenterControl.Services;

namespace IntelliCenterControl.Models
{
    public class Body : Circuit<IntelliCenterConnection>
    {
        public const string BodyKeys = "[\"TEMP\",\"STATUS\",\"HTMODE\",\"MODE\",\"LSTTMP\",\"HTSRC\", \"HITMP\", \"LOTMP\"]";

        public enum BodyType
        {
            [Description( "Pool")]
            POOL,
            [Description( "Spa")]
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
                _temp = value;
                OnPropertyChanged();
            }
        }

        private string _lastTemp;

        public string LastTemp
        {
            get => _lastTemp;
            set
            {
                _lastTemp = value;
                OnPropertyChanged();
            }
        }

        private HeatModes heatMode;

        public HeatModes HeatMode
        {
            get => heatMode;
            set
            {
                heatMode = value;
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
                ExecuteSelectedHeatSourceCommand();
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
                if(int.TryParse(_loTemp,out var iloTemp ))
                {
                    ExecuteHeatTempCommand(iloTemp);
                }
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



        public Body(string name, BodyType bodyType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) :base(name, CircuitType.BODY, hName, dataInterface)
        {
            Type = bodyType;
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

        private async Task ExecuteHeatTempCommand(int temp)
        {
            if (DataInterface != null && temp >= 60 && temp <= 104)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "LOTMP", temp.ToString());
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "BODY");
            }
        }

    }
}

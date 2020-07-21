using System;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IntelliCenterControl.Models
{
    public class Pump : Circuit<IntelliCenterConnection>
    {
        public const string PumpKeys = "[\"RPM\", \"GPM\", \"PWR\",\"STATUS\"]";

        public enum PumpType
        {
            [Description("Single Speed")]
            SINGLE,
            [Description("Dual Speed")]
            DUAL,
            [Description("Variable Speed")]
            SPEED,
            [Description("Variable GPM")]
            FLOW,
            [Description("Variable Speed and GPM")]
            VSF
        }

        public enum PumpStatus
        {
            OFF = 4,
            ON = 10
        }

        private PumpType _type;

        public PumpType Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        private PumpStatus pumpStatus;

        public PumpStatus Status
        {
            get => pumpStatus;
            set
            {
                pumpStatus = value;
                this.UpdateActiveState(PumpStatus.ON == value);
                OnPropertyChanged();
            }
        }


        private int _primeSpeed;

        public int PrimeSpeed
        {
            get => _primeSpeed;
            set
            {
                if (_primeSpeed == value) return;
                _primeSpeed = value;
                OnPropertyChanged();
            }
        }

        private int _minSpeed;

        public int MinSpeed
        {
            get => _minSpeed;
            set
            {
                if (_minSpeed == value) return;
                _minSpeed = value;
                OnPropertyChanged();
            }
        }

        private int _maxSpeed;

        public int MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                if (_maxSpeed == value) return;
                _maxSpeed = value;
                OnPropertyChanged();
            }
        }

        private int _minFlow;

        public int MinFlow
        {
            get => _minFlow;
            set
            {
                if (_minFlow == value) return;
                _minFlow = value;
                OnPropertyChanged();
            }
        }

        private int _maxFlow;

        public int MaxFlow
        {
            get => _maxFlow;
            set
            {
                if (_maxFlow == value) return;
                _maxFlow = value;
                OnPropertyChanged();
            }
        }

        private string _rpm = "-";

        public string RPM
        {
            get => _rpm;
            set
            {
                if (_rpm == value) return;
                _rpm = value;
                OnPropertyChanged();
            }
        }

        private string _gpm = "-";

        public string GPM
        {
            get => _gpm;
            set
            {
                if (_gpm == value) return;
                _gpm = value;
                OnPropertyChanged();
            }
        }

        private string _power = "-";

        public string Power
        {
            get => _power;
            set
            {
                if (_power == value) return;
                _power = value;
                OnPropertyChanged();
            }
        }


        public Pump(string name, string Hname) : base(name, CircuitType.PUMP, Hname)
        {

        }

        public override async Task UpdateItemAsync(JObject data)
        {
            if (data.TryGetValue("params", out var pumpValues))
            {
                var pv = (JObject)pumpValues;
                if (pv.TryGetValue("RPM", out var rpm))
                {
                    RPM = rpm.ToString() == "0" ? "-" : rpm.ToString();
                }

                if (pv.TryGetValue("GPM", out var flow))
                {
                    GPM = flow.ToString() == "0" ? "-" : flow.ToString();
                }

                if (pv.TryGetValue("PWR", out var power))
                {
                    Power = power.ToString() == "0"
                        ? "-"
                        : power.ToString();
                }

                if (pv.TryGetValue("STATUS", out var status))
                {
                    var ps = (int)status;
                    if (Enum.IsDefined(typeof(Pump.PumpStatus), ps))
                        Status = (Pump.PumpStatus)ps;
                    else
                        Status = Pump.PumpStatus.OFF;
                }

            }
        }
    }
}

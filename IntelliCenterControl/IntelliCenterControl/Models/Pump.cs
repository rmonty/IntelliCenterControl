using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Pump : Circuit
    {
        public const string PumpKeys = "[\"RPM\", \"GPM\", \"PWR\",\"STATUS\"]";

        public enum PumpType
        {
            [Display(Name = "Single Speed")]
            SINGLE,
            [Display(Name = "Dual Speed")]
            DUAL,
            [Display(Name = "Variable Speed")]
            SPEED,
            [Display(Name = "Variable GPM")]
            FLOW,
            [Display(Name = "Variable Speed and GPM")]
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
                switch (value)
                {
                    case PumpStatus.ON:
                        Active = true;
                        break;
                    case PumpStatus.OFF:
                        Active = false;
                        break;
                    default: break;
                }

                OnPropertyChanged();
            }
        }


        private int _primeSpeed;

        public int PrimeSpeed
        {
            get => _primeSpeed;
            set
            {
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
                _maxFlow = value;
                OnPropertyChanged();
            }
        }

        private string _rpm;

        public string RPM
        {
            get => _rpm;
            set
            {
                _rpm = value;
                OnPropertyChanged();
            }
        }

        private string _gpm;

        public string GPM
        {
            get => _gpm;
            set
            {
                _gpm = value;
                OnPropertyChanged();
            }
        }

        private string _power;

        public string Power
        {
            get => _power;
            set
            {
                _power = value;
                OnPropertyChanged();
            }
        }
        

        public Pump(string name, string Hname) : base(name, CircuitType.PUMP, Hname)
        {

        }
    }
}

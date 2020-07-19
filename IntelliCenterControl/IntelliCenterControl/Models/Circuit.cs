using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IntelliCenterControl.Annotations;
using IntelliCenterControl.Services;

namespace IntelliCenterControl.Models
{
    public class Circuit<T> : INotifyPropertyChanged
    {
        public const string CircuitKeys = "[\"STATUS\", \"MODE\"]";

        public enum CircuitType
        {
            [Description( "Body")]
            BODY,
            [Description( "Circuit")]
            CIRCUIT,
            [Description( "Heater")]
            HEATER,
            [Description( "Remote")]
            REMOTE,
            [Description( "Chem Relay")]
            CHEM,
            [Description( "Circuit Group")]
            CIRCGRP,
            [Description( "Generic")]
            GENERIC,
            [Description( "Spillway")]
            SPILL,
            [Description( "Pump")]
            PUMP,
            [Description( "Sense")]
            SENSE,
            [Description( "Module")]
            MODULE,
            [Description( "Pool")]
            POOL,
            [Description( "Dimmer")]
            DIMMER,
            [Description( "GloBrite")]
            GLOW,
            [Description( "GloBrite White")]
            GLOWT,
            [Description( "IntelliBrite")]
            INTELLI,
            [Description( "Light")]
            LIGHT,
            [Description( "Magic Stream")]
            MAGIC2,
            [Description( "Color Cascade")]
            CLRCASC,
            [Description( "Schedule")]
            SCHED,
            [Description( "Panel")]
            PANEL,
            [Description( "Relay")]
            RLY,
            [Description( "Legacy")]
            LEGACY,
            [Description("Spa")]
            SPA

        }

        private CircuitType _circuitDescription;

        public CircuitType CircuitDescription
        {
            get => _circuitDescription;
            set
            {
                _circuitDescription = value;
                OnPropertyChanged();
            }
        }

        
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _hName;

        public string Hname
        {
            get => _hName;
            set
            {
                _hName = value;
                OnPropertyChanged();
            }
        }


        private bool _active;

        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value) return;
                _active = value;
                
                OnPropertyChanged();
                ExecuteToggleCircuitCommand();

            }
        }

        private bool _display;

        public bool Display
        {
            get => _display;
            set
            {
                _display = value;
                OnPropertyChanged();
            }
        }

        private int _listOrd;

        public int ListOrd
        {
            get => _listOrd;
            set
            {
                _listOrd = value;
                OnPropertyChanged();
            }
        }

        public IDataInterface<T> DataInterface { get; private set; }

        public Circuit(string name, CircuitType circuitType, string hName = "",
            IDataInterface<T> dataInterface = null)
        {
            DataInterface = dataInterface;
            Name = name;
            Hname = hName;
            CircuitDescription = circuitType;
        }

        protected Circuit(CircuitType circuitType, IDataInterface<T> dataInterface = null)
        {
            DataInterface = dataInterface;
        }


        protected virtual async Task ExecuteToggleCircuitCommand()
        {
            if (DataInterface != null)
            {
                var val = Active ? "ON" : "OFF";
                if (!await DataInterface.SendItemParamsUpdateAsync(Hname, "STATUS", val))
                {
                    Active = false;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}

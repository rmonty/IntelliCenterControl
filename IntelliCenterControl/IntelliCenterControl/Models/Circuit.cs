using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IntelliCenterControl.Annotations;
using IntelliCenterControl.Services;
using Xamarin.Forms;

namespace IntelliCenterControl.Models
{
    public class Circuit : INotifyPropertyChanged
    {
        public const string CircuitKeys = "[\"STATUS\", \"MODE\"]";

        public enum CircuitType
        {
            [Display(Name = "Body")]
            BODY,
            [Display(Name = "Circuit")]
            CIRCUIT,
            [Display(Name = "Heater")]
            HEATER,
            [Display(Name = "Remote")]
            REMOTE,
            [Display(Name = "Chem Relay")]
            CHEM,
            [Display(Name = "Circuit Group")]
            CIRCGRP,
            [Display(Name = "Generic")]
            GENERIC,
            [Display(Name = "Spillway")]
            SPILL,
            [Display(Name = "Pump")]
            PUMP,
            [Display(Name = "Sense")]
            SENSE,
            [Display(Name = "Module")]
            MODULE,
            [Display(Name = "Pool")]
            POOL,
            [Display(Name = "Dimmer")]
            DIMMER,
            [Display(Name = "GloBrite")]
            GLOW,
            [Display(Name = "GloBrite White")]
            GLOWT,
            [Display(Name = "IntelliBrite")]
            INTELLI,
            [Display(Name = "Light")]
            LIGHT,
            [Display(Name = "Magic Stream")]
            MAGIC2,
            [Display(Name = "Color Cascade")]
            CLRCASC,
            [Display(Name = "Schedule")]
            SCHED,
            [Display(Name = "Panel")]
            PANEL,
            [Display(Name = "Relay")]
            RLY,
            [Display(Name = "Legacy")]
            LEGACY

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

        public IDataInterface<HardwareDefinition> DataInterface { get; private set; }

        public Circuit(string name, CircuitType circuitType, string hName = "",
            IDataInterface<HardwareDefinition> dataInterface = null)
        {
            DataInterface = dataInterface;
            Name = name;
            Hname = hName;
            CircuitDescription = circuitType;
        }

        private async Task ExecuteToggleCircuitCommand()
        {
            if (DataInterface != null)
            {
                await DataInterface.UnSubscribeItemUpdate(Hname);
                var val = _active ? "ON" : "OFF";
                await DataInterface.SendItemUpdateAsync(Hname, "STATUS", val);
                await DataInterface.SubscribeItemUpdateAsync(Hname, CircuitDescription.ToString());
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

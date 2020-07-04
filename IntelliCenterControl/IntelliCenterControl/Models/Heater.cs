using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Heater : Circuit
    {
        public enum HeaterType
        {
            [Display(Name = "Generic")]
            GENERIC,
            [Display(Name = "Solar")]
            SOLAR,
            [Display(Name = "Heat Pump")]
            HTPMP,
            [Display(Name = "UltraTemp")]
            ULTRA,
            [Display(Name = "MasterTemp")]
            MASTER,
            [Display(Name = "Max-E-Therm")]
            MAX,
            [Display(Name = "Hybrid")]
            HCOMBO
        }

        private HeaterType _type;

        public HeaterType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public Heater(string name, HeaterType heaterType) : base(name, CircuitType.HEATER)
        {
            Type = heaterType;
        }

    }
}

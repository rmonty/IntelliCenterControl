using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Valve : Circuit
    {
        public enum ValveType
        {
            [Display(Name = "Legacy")]
            LEGACY,
            [Display(Name = "Intellivalve")]
            INTELLI
        }

        private ValveType _type;

        public ValveType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public Valve(string name, ValveType valveType) : base(name, CircuitType.LEGACY)
        {
            Type = valveType;
        }

    }
}

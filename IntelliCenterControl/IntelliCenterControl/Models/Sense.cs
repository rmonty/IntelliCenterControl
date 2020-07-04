using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Sense : Circuit
    {
        public enum SenseType
        {
            [Display(Name = "Air")]
            AIR,
            [Display(Name = "Solar")]
            SOLAR,
            [Display(Name = "Water")]
            POOL
        }

        private SenseType _type;

        public SenseType Type
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

        public Sense(string name, SenseType senseType):base(name, CircuitType.SENSE)
        {
            Type = senseType;
        }

    }
}

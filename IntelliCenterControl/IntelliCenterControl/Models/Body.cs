using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;
using IntelliCenterControl.Services;

namespace IntelliCenterControl.Models
{
    public class Body : Circuit
    {
        public enum BodyType
        {
            [Display(Name = "Pool")]
            POOL,
            [Display(Name = "Spa")]
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





        public Body(string name, BodyType bodyType):base(name, CircuitType.BODY)
        {
            Type = bodyType;
        }

    }
}

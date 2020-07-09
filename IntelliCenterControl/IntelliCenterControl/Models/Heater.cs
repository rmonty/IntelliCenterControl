using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;
using IntelliCenterControl.Services;

namespace IntelliCenterControl.Models
{
    public class Heater : Circuit
    {
        public const string HeaterKeys = "[\"STATUS\", \"SUBTYP\", \"PERMIT\", \"TIMOUT\", \"READY\", \"HTMODE\", \"SHOMNU\", \"COOL\", \"COMUART\", \"BODY\", \"HNAME\", \"START\", \"STOP\", \"HEATING\",\"BOOST\",\"TIME\",\"DLY\"]";

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

        private IList<string> _bodies;

        public IList<string> Bodies
        {
            get => _bodies;
            set
            {
                _bodies = value;
                OnPropertyChanged();
            }
        }



        public Heater(string name, HeaterType heaterType, string hName, IDataInterface<HardwareDefinition> dataInterface) : base(name, CircuitType.HEATER, hName, dataInterface)
        {
            Type = heaterType;
        }

    }
}

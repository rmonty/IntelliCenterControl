using IntelliCenterControl.Services;
using System.Collections.Generic;
using System.ComponentModel;

namespace IntelliCenterControl.Models
{
    public class Heater : Circuit<IntelliCenterConnection>
    {
        public const string HeaterKeys = "[\"STATUS\", \"SUBTYP\", \"PERMIT\", \"TIMOUT\", \"READY\", \"HTMODE\", \"SHOMNU\", \"COOL\", \"COMUART\", \"BODY\", \"HNAME\", \"START\", \"STOP\", \"HEATING\",\"BOOST\",\"TIME\",\"DLY\"]";

        public enum HeaterType
        {
            [Description("Generic")]
            GENERIC,
            [Description("Solar")]
            SOLAR,
            [Description("Heat Pump")]
            HTPMP,
            [Description("UltraTemp")]
            ULTRA,
            [Description("MasterTemp")]
            MASTER,
            [Description("Max-E-Therm")]
            MAX,
            [Description("Hybrid")]
            HCOMBO
        }

        private HeaterType _type;

        public HeaterType Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
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
                if (Equals(_bodies, value)) return;
                _bodies = value;
                OnPropertyChanged();
            }
        }



        public Heater(string name, HeaterType heaterType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, CircuitType.HEATER, hName, dataInterface)
        {
            Type = heaterType;
        }

    }
}

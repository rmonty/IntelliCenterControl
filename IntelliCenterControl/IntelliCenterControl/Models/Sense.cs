using System.ComponentModel;

namespace IntelliCenterControl.Models
{
    public class Sense : Circuit<IntelliCenterConnection>
    {
        public const string SenseKeys = "[\"PROBE\", \"STATUS\"]";

        public enum SenseType
        {
            [Description( "Air")]
            AIR,
            [Description( "Solar")]
            SOLAR,
            [Description( "Water")]
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

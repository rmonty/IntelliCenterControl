using System.ComponentModel;

namespace IntelliCenterControl.Models
{
    public class Valve : Circuit<IntelliCenterConnection>
    {
        public enum ValveType
        {
            [Description( "Legacy")]
            LEGACY,
            [Description( "Intellivalve")]
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

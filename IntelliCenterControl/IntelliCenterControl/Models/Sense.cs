using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IntelliCenterControl.Models
{
    public class Sense : Circuit<IntelliCenterConnection>
    {
        public const string SenseKeys = "[\"PROBE\", \"STATUS\"]";

        public enum SenseType
        {
            [Description("Air")]
            AIR,
            [Description("Solar")]
            SOLAR,
            [Description("Water")]
            POOL
        }

        private SenseType _type;

        public SenseType Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
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
                if (_temp == value) return;
                _temp = value;
                OnPropertyChanged();
            }
        }

        public Sense(string name, SenseType senseType) : base(name, CircuitType.SENSE)
        {
            Type = senseType;
        }

        public override async Task UpdateItemAsync(JObject data)
        {
            if (data.TryGetValue("params", out var senseValues))
            {
                var sv = (JObject)senseValues;

                if (sv.TryGetValue("PROBE", out var temp))
                {
                    if (int.TryParse(temp.ToString(), out var itemp))
                    {
                        Temp = itemp;
                    }
                }
            }

        }

    }
}

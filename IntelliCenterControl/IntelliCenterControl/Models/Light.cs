using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IntelliCenterControl.Services;

namespace IntelliCenterControl.Models
{
    public class Light : Circuit<IntelliCenterConnection>
    {
        public const string LightKeys = "[\"ACT\", \"USE\"]";

        public enum LightType
        {
            [Description( "Dimmer")]
            [Color(false)]
            [Dimming(true)]
            DIMMER,
            [Description( "GloBrite")]
            [Color(true)]
            [Dimming(false)]
            GLOW,
            [Description( "GloBrite White")]
            [Color(false)]
            [Dimming(true)]
            GLOWT,
            [Description( "IntelliBrite")]
            [Color(true)]
            [Dimming(false)]
            INTELLI,
            [Description( "Light")]
            [Color(false)]
            [Dimming(false)]
            LIGHT,
            [Description( "Magic Stream")]
            [Color(true)]
            [Dimming(false)]
            MAGIC2,
            [Description("Color Cascade")]
            [Color(true)]
            [Dimming(false)]
            CLRCASC
        }

        public enum LightColors
        {
            [Description("SAm")]
            SAMMOD,
            [Description("Party")]
            PARTY,
            [Description("Romance")]
            ROMAN,
            [Description("Caribbean")]
            CARIB,
            [Description("American")]
            AMERCA,
            [Description("Sunset")]
            SSET,
            [Description("Royal")]
            ROYAL,
            [Description("Blue")]
            BLUER,
            [Description("Green")]
            GREENR,
            [Description("Red")]
            REDR,
            [Description("White")]
            WHITER,
            [Description("Magenta")]
            MAGNTAR
        }

        private LightType _type;

        public LightType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public List<string> ColorNames => GetDescriptions(typeof(LightColors)).ToList();

        private LightColors _color = LightColors.WHITER;

        public LightColors Color
        {
            get => _color;
            set
            {
                if (!SupportsColor) return;
                if (_color == value) return;
                _color = value;
                OnPropertyChanged();
                ExecuteLightColorCommand();
            }
        }

        private int _dimmingValue = 30;

        public int DimmingValue
        {
            get => _dimmingValue;
            set
            {
                if (!SupportsDimming) return;
                if (_dimmingValue == value) return;
                _dimmingValue = value < MinDimmingValue ? MinDimmingValue : value;

                OnPropertyChanged();
                ExecuteDimmerValueCommand();
            }
        }

        private int _minDimmingValue;

        public int MinDimmingValue
        {
            get => _minDimmingValue;
            set
            {
                _minDimmingValue = value;
                OnPropertyChanged();
            }
        }

        private double _dimmingIncrement;

        public double DimmingIncrement
        {
            get => _dimmingIncrement;
            set
            {
                _dimmingIncrement = value;
                OnPropertyChanged();
            }
        }



        public bool SupportsColor => GetAttribute<ColorAttribute>(Type).SupportsColor;

        public bool SupportsDimming => GetAttribute<DimmingAttribute>(Type).SupportsDimming;

        public Light(string name, LightType lightType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, (CircuitType)Enum.Parse(typeof(CircuitType),lightType.ToString()), hName, dataInterface)
        {
            Type = lightType;

            switch (lightType)
            {
                case LightType.DIMMER:
                    MinDimmingValue = 30;
                    DimmingIncrement = 10;
                    break;
                case LightType.GLOWT:
                    MinDimmingValue = 50;
                    DimmingIncrement = 25;
                    break;
                default:
                    break;
            }
        }


        protected override async Task ExecuteToggleCircuitCommand()
        {
            if (DataInterface != null)
            {
               // await DataInterface.UnSubscribeItemUpdate(Hname);
                var val = Active ? "ON" : "OFF";
                await DataInterface.SendItemParamsUpdateAsync(Hname, "STATUS", val);
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                //await DataInterface.SubscribeItemUpdateAsync(Hname, CircuitDescription.ToString());
            }
        }

        protected async Task ExecuteLightColorCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "ACT", Color.ToString());
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
            }
        }

        protected async Task ExecuteDimmerValueCommand()
        {
            if (DataInterface != null)
            {
                //await DataInterface.UnSubscribeItemUpdate(Hname);
                await DataInterface.SendItemParamsUpdateAsync(Hname, "ACT", DimmingValue.ToString());
                //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
                await DataInterface.GetItemUpdateAsync(Hname, CircuitDescription.ToString());
            }
        }

        internal class ColorAttribute : Attribute
        {
            public bool SupportsColor { get; private set; }

            public ColorAttribute(bool supportsColor)
            {
                SupportsColor = supportsColor;
            }
        }

        internal class DimmingAttribute : Attribute
        {
            public bool SupportsDimming { get; private set; }

            public DimmingAttribute(bool supportsDimming)
            {
                SupportsDimming = supportsDimming;
            }
        }


        public static TAttribute GetAttribute<TAttribute>(Enum value)
            where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        private static IEnumerable<string> GetDescriptions(Type type)
        {
            var descs = new List<string>();
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var field = type.GetField(name);
                var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                foreach (DescriptionAttribute fd in fds)
                {
                    descs.Add(fd.Description);
                }
            }
            return descs;
        }

    }



   
}

using IntelliCenterControl.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliCenterControl.Models
{
    public class Light : Circuit<IntelliCenterConnection>
    {
        public const string LightKeys = "[\"STATUS\",\"MODE\",\"LISTORD\",\"USAGE\",\"FREEZE\",\"LIMIT\",\"USE\",\"MANUAL\",\"FEATR\",\"DNTSTP\",\"CHILD\",\"HNAME\",\"SNAME\",\"RLY\",\"OBJNAM\",\"OBJTYP\",\"SHOMNU\",\"TIME\",\"TIMOUT\",\"SOURCE\",\"SUBTYP\",\"BODY\"]";

        public enum LightType
        {
            [Description("Dimmer")]
            [Color(false)]
            [Dimming(true)]
            DIMMER,
            [Description("GloBrite")]
            [Color(true)]
            [Dimming(false)]
            GLOW,
            [Description("GloBrite White")]
            [Color(false)]
            [Dimming(true)]
            GLOWT,
            [Description("IntelliBrite")]
            [Color(true)]
            [Dimming(false)]
            INTELLI,
            [Description("Light")]
            [Color(false)]
            [Dimming(false)]
            LIGHT,
            [Description("Magic Stream")]
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
                if (_type == value) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        public List<string> ColorNames => EnumHelpers.GetDescriptions(typeof(LightColors)).ToList();

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
                if (SendColorStateUpdate) Task.Run(ExecuteLightColorCommand);
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
                Task.Run(ExecuteDimmerValueCommand);
            }
        }

        private int _minDimmingValue;

        public int MinDimmingValue
        {
            get => _minDimmingValue;
            set
            {
                if (_minDimmingValue == value) return;
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
                if (_dimmingIncrement == value) return;
                _dimmingIncrement = value;
                OnPropertyChanged();
            }
        }



        public bool SupportsColor => EnumHelpers.GetAttribute<ColorAttribute>(Type).SupportsColor;

        public bool SupportsDimming => EnumHelpers.GetAttribute<DimmingAttribute>(Type).SupportsDimming;

        private bool SendColorStateUpdate = true;

        public Light(string name, LightType lightType, string hName, IDataInterface<IntelliCenterConnection> dataInterface) : base(name, (CircuitType)Enum.Parse(typeof(CircuitType), lightType.ToString()), hName, dataInterface)
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
            }
        }


        //protected override async Task ExecuteToggleCircuitCommand()
        //{
        //    if (DataInterface != null)
        //    {
        //        // await DataInterface.UnSubscribeItemUpdate(Hname);
        //        var val = Active ? "ON" : "OFF";
        //        await DataInterface.SendItemParamsUpdateAsync(Hname, "STATUS", val);
        //        //await DataInterface.SubscribeItemUpdateAsync(Hname, "CIRCUIT");
        //        //await DataInterface.SubscribeItemUpdateAsync(Hname, CircuitDescription.ToString());
        //    }
        //}

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

        public void UpdateColorState(LightColors color)
        {
            SendColorStateUpdate = false;
            Color = color;
            SendColorStateUpdate = true;
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

        public override async Task UpdateItemAsync(JObject data)
        {

            if (data.TryGetValue("params", out var lightValues))
            {
                var lv = (JObject)lightValues;

                await base.UpdateItemAsync(data);

                if (lv.TryGetValue("USE", out var lightColor))
                {
                    if (Enum.TryParse<LightColors>(lightColor.ToString(),
                        out var color))
                    {
                        UpdateColorState(color);
                    }
                }
            }

        }


    }




}

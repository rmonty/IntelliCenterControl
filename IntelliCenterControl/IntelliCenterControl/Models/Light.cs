using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Light : Circuit
    {
        public enum LightType
        {
            [Display(Name = "Dimmer")]
            [Color(false)]
            DIMMER,
            [Display(Name = "GloBrite")]
            [Color(true)]
            GLOW,
            [Display(Name = "GloBrite White")]
            [Color(false)]
            GLOWT,
            [Display(Name = "IntelliBrite")]
            [Color(true)]
            INTELLI,
            [Display(Name = "Light")]
            [Color(false)]
            LIGHT,
            [Display(Name = "Magic Stream")]
            [Color(true)]
            MAGIC2,
            [Display(Name = "Color Cascade")]
            [Color(true)]
            CLRCASC
        }

        public enum LightColors
        {
            [Display(Name = "SAm")]
            SAMMOD,
            [Display(Name = "Party")]
            PARTY,
            [Display(Name = "Romance")]
            ROMAN,
            [Display(Name = "Caribbean")]
            CARIB,
            [Display(Name = "American")]
            AMERCA,
            [Display(Name = "Sunset")]
            SSET,
            [Display(Name = "Royal")]
            ROYAL,
            [Display(Name = "Blue")]
            BLUER,
            [Display(Name = "Green")]
            GREENR,
            [Display(Name = "Red")]
            REDR,
            [Display(Name = "White")]
            WHITER,
            [Display(Name = "Magenta")]
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

        private LightColors _color = LightColors.WHITER;

        public LightColors Color
        {
            get => _color;
            set
            {
                if (!SupportsColor) return;
                _color = value;
                OnPropertyChanged();
            }
        }

        public bool SupportsColor => GetAttribute<ColorAttribute>(Color).SupportsColor;

        public Light(string name, LightType lightType) : base(name, Enum.Parse<CircuitType>(lightType.ToString()))
        {
            Type = lightType;
        }

        internal class ColorAttribute : Attribute
        {
            public bool SupportsColor { get; private set; }

            public ColorAttribute(bool supportsColor)
            {
                SupportsColor = supportsColor;
            }
        }


        public static TAttribute GetAttribute<TAttribute>(Enum value)
            where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }
    }
}

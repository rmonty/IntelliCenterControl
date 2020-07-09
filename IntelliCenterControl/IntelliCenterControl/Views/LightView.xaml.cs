using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliCenterControl.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightView : ContentView
    {
        public LightView()
        {
            InitializeComponent();
            
        }

        private void ColorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var color = (Light.LightColors) ColorPicker.SelectedIndex;
            switch (color)
            {
                case Light.LightColors.SAMMOD:
                    ColorPicker.BackgroundColor = Color.LightGray;
                    break;
                case Light.LightColors.PARTY:
                    ColorPicker.BackgroundColor = Color.HotPink;
                    break;
                case Light.LightColors.ROMAN:
                    ColorPicker.BackgroundColor = Color.LightBlue;
                    break;
                case Light.LightColors.CARIB:
                    ColorPicker.BackgroundColor = Color.GreenYellow;
                    break;
                case Light.LightColors.AMERCA:
                    ColorPicker.BackgroundColor = Color.BlueViolet;
                    break;
                case Light.LightColors.SSET:
                    ColorPicker.BackgroundColor = Color.OrangeRed;
                    break;
                case Light.LightColors.ROYAL:
                    ColorPicker.BackgroundColor = Color.RoyalBlue;
                    break;
                case Light.LightColors.BLUER:
                    ColorPicker.BackgroundColor = Color.Blue;
                    break;
                case Light.LightColors.GREENR:
                    ColorPicker.BackgroundColor = Color.Green;
                    break;
                case Light.LightColors.REDR:
                    ColorPicker.BackgroundColor = Color.Red;
                    break;
                case Light.LightColors.WHITER:
                    ColorPicker.BackgroundColor = Color.White;
                    break;
                case Light.LightColors.MAGNTAR:
                    ColorPicker.BackgroundColor = Color.Magenta;
                    break;
                default:
                    ColorPicker.BackgroundColor = Color.Transparent;
                    break;
            }
        }
    }

    public class IntEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                return (int)value;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                return Enum.ToObject(targetType, value);
            }
            return 0;
        }
    }

}
using IntelliCenterControl.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightView : ContentView
    {
        private LightColors _colors = LightColors.Instance;
        
        public LightView()
        {
            InitializeComponent();
        }

        private void ColorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var color = (Light.LightColors)ColorPicker.SelectedIndex;
            switch (color)
            {
                case Light.LightColors.SAMMOD:
                    ColorBox.Background = _colors.Sam;
                    break;
                case Light.LightColors.PARTY:
                    ColorBox.Background = _colors.Party;
                    break;
                case Light.LightColors.ROMAN:
                    ColorBox.Background = _colors.Romance;
                    break;
                case Light.LightColors.CARIB:
                    ColorBox.Background = _colors.Caribbean;
                    break;
                case Light.LightColors.AMERCA:
                    ColorBox.Background = _colors.American;
                    break;
                case Light.LightColors.SSET:
                    ColorBox.Background = _colors.Sunset;
                    break;
                case Light.LightColors.ROYAL:
                    ColorBox.Background = _colors.Royal;
                    break;
                case Light.LightColors.BLUER:
                    ColorBox.Background = _colors.Blue;
                    break;
                case Light.LightColors.GREENR:
                    ColorBox.Background = _colors.Green;
                    break;
                case Light.LightColors.REDR:
                    ColorBox.Background = _colors.Red;
                    break;
                case Light.LightColors.WHITER:
                    ColorBox.Background = _colors.White;
                    break;
                case Light.LightColors.MAGNTAR:
                    ColorBox.Background = _colors.Magenta;
                    break;
                default:
                    ColorBox.Background = _colors.White;
                    break;
            }
        }
    }



}
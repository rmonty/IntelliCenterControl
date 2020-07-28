using IntelliCenterControl.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightGroupView : ContentView
    {
        public LightGroupView()
        {
            InitializeComponent();

        }

        private void ColorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var color = (Light.LightColors)ColorPicker.SelectedIndex;
            switch (color)
            {
                case Light.LightColors.SAMMOD:
                    ColorBox.BackgroundColor = Color.LightGray;
                    break;
                case Light.LightColors.PARTY:
                    ColorBox.BackgroundColor = Color.HotPink;
                    break;
                case Light.LightColors.ROMAN:
                    ColorBox.BackgroundColor = Color.LightBlue;
                    break;
                case Light.LightColors.CARIB:
                    ColorBox.BackgroundColor = Color.GreenYellow;
                    break;
                case Light.LightColors.AMERCA:
                    ColorBox.BackgroundColor = Color.BlueViolet;
                    break;
                case Light.LightColors.SSET:
                    ColorBox.BackgroundColor = Color.OrangeRed;
                    break;
                case Light.LightColors.ROYAL:
                    ColorBox.BackgroundColor = Color.RoyalBlue;
                    break;
                case Light.LightColors.BLUER:
                    ColorBox.BackgroundColor = Color.Blue;
                    break;
                case Light.LightColors.GREENR:
                    ColorBox.BackgroundColor = Color.Green;
                    break;
                case Light.LightColors.REDR:
                    ColorBox.BackgroundColor = Color.Red;
                    break;
                case Light.LightColors.WHITER:
                    ColorBox.BackgroundColor = Color.White;
                    break;
                case Light.LightColors.MAGNTAR:
                    ColorBox.BackgroundColor = Color.Magenta;
                    break;
                default:
                    ColorBox.BackgroundColor = Color.Transparent;
                    break;
            }
        }
    }



}
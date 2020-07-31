
using System;
using IntelliCenterControl.Models;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LightGroupSettingsPage : Rg.Plugins.Popup.Pages.PopupPage
    {
        private LightColors _colors = LightColors.Instance;

        public LightGroupSettingsPage()
        {
            InitializeComponent();
        }

        public LightGroupSettingsPage(object context)
        {
            BindingContext = context;

            InitializeComponent();
        }

        // Invoked when a hardware back button is pressed
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            return false;
        }

        //// Invoked when background is clicked
        //protected override bool OnBackgroundClicked()
        //{
        //    // Return false if you don't want to close this popup page when a background of the popup page is clicked
        //    return false;
        //}

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            Navigation.RemovePopupPageAsync(this);
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
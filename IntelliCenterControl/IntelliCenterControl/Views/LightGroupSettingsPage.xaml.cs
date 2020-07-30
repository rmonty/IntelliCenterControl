
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
        private readonly LinearGradientBrush _party = new LinearGradientBrush();
        private readonly LinearGradientBrush _american = new LinearGradientBrush();
        private readonly LinearGradientBrush _caribbean = new LinearGradientBrush();
        private readonly LinearGradientBrush _sunset = new LinearGradientBrush();
        private readonly LinearGradientBrush _romance = new LinearGradientBrush();
        private readonly LinearGradientBrush _royal = new LinearGradientBrush();
        private readonly LinearGradientBrush _magenta = new LinearGradientBrush();
        private readonly LinearGradientBrush _red = new LinearGradientBrush();
        private readonly LinearGradientBrush _green = new LinearGradientBrush();
        private readonly LinearGradientBrush _blue = new LinearGradientBrush();
        private readonly LinearGradientBrush _white = new LinearGradientBrush();
        private readonly LinearGradientBrush _sam = new LinearGradientBrush();

        public LightGroupSettingsPage()
        {
            InitializeComponent();
        }

        public LightGroupSettingsPage(object context)
        {
            BindingContext = context;

             _party.GradientStops.Add(new GradientStop(Color.FromHex("E7322F"), 0f));
            _party.GradientStops.Add(new GradientStop(Color.FromHex("ea2132"), 0.2f));
            _party.GradientStops.Add(new GradientStop(Color.FromHex("cb5ea7"), 0.4f));
            _party.GradientStops.Add(new GradientStop(Color.FromHex("59449c"), 0.6f));
            _party.GradientStops.Add(new GradientStop(Color.FromHex("4cb85b"), 0.8f));
            _party.GradientStops.Add(new GradientStop(Color.FromHex("f7f597"), 1f));
            _party.SetValue(RotationProperty, 45);

            _american.GradientStops.Add(new GradientStop(Color.FromHex("260d2b"), 0f));
            _american.GradientStops.Add(new GradientStop(Color.FromHex("006399"), 0.2f));
            _american.GradientStops.Add(new GradientStop(Color.FromHex("D52E5F"), 0.4f));
            _american.GradientStops.Add(new GradientStop(Color.FromHex("260d2b"), 0.6f));
            _american.GradientStops.Add(new GradientStop(Color.FromHex("006399"), 0.8f));
            _american.GradientStops.Add(new GradientStop(Color.FromHex("D52E5F"), 1f));
            _american.SetValue(RotationProperty, 45);

            _caribbean.GradientStops.Add(new GradientStop(Color.FromHex("184937"), 0f));
            _caribbean.GradientStops.Add(new GradientStop(Color.FromHex("58c872"), 0.2f));
            _caribbean.GradientStops.Add(new GradientStop(Color.FromHex("2b398a"), 0.4f));
            _caribbean.GradientStops.Add(new GradientStop(Color.FromHex("cd62b3"), 0.6f));
            _caribbean.GradientStops.Add(new GradientStop(Color.FromHex("58c872"), 0.8f));
            _caribbean.GradientStops.Add(new GradientStop(Color.FromHex("f3f6a1"), 1f));
            _caribbean.SetValue(RotationProperty, 45);

            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("FDBA56"), 0f));
            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("D3535C"), 0.15f));
            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("683B93"), 0.3f));
            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("EA832A"), 0.45f));
            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("7C73B4"), 0.60f));
            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("F6644E"), 0.75f));
            _sunset.GradientStops.Add(new GradientStop(Color.FromHex("FDBF63"), 1f));
            _sunset.SetValue(RotationProperty, 45);

            _romance.GradientStops.Add(new GradientStop(Color.FromHex("87d5e1"), 0f));
            _romance.GradientStops.Add(new GradientStop(Color.FromHex("6d79c3"), 0.5f));
            _romance.GradientStops.Add(new GradientStop(Color.FromHex("aa67b5"), 1f));
            _romance.SetValue(RotationProperty, 45);

            _royal.GradientStops.Add(new GradientStop(Color.FromHex("FFF165"), 0f));
            _royal.GradientStops.Add(new GradientStop(Color.FromHex("1DA453"), 0.5f));
            _royal.GradientStops.Add(new GradientStop(Color.FromHex("2E3192"), 1f));
            _royal.SetValue(RotationProperty, 45);

            _magenta.GradientStops.Add(new GradientStop(Color.FromHex("EC008B"), 0f));

            _red.GradientStops.Add(new GradientStop(Color.Red, 0f));

            _green.GradientStops.Add(new GradientStop(Color.Green, 0f));

            _blue.GradientStops.Add(new GradientStop(Color.Blue, 0f));

            _white.GradientStops.Add(new GradientStop(Color.White, 0f));

            _sam.GradientStops.Add(new GradientStop(Color.Red, 0f));
            _sam.GradientStops.Add(new GradientStop(Color.Green, 0.25f));
            _sam.GradientStops.Add(new GradientStop(Color.Blue, 0.5f));
            _sam.GradientStops.Add(new GradientStop(Color.White, 0.75f));
            _sam.GradientStops.Add(new GradientStop(Color.FromHex("EC008B"), 1f));
            _sam.SetValue(RotationProperty, 45);

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
                    ColorBox.Background = _sam;
                    break;
                case Light.LightColors.PARTY:
                    ColorBox.Background = _party;
                    break;
                case Light.LightColors.ROMAN:
                    ColorBox.Background = _romance;
                    break;
                case Light.LightColors.CARIB:
                    ColorBox.Background = _caribbean;
                    break;
                case Light.LightColors.AMERCA:
                    ColorBox.Background = _american;
                    break;
                case Light.LightColors.SSET:
                    ColorBox.Background = _sunset;
                    break;
                case Light.LightColors.ROYAL:
                    ColorBox.Background = _royal;
                    break;
                case Light.LightColors.BLUER:
                    ColorBox.Background = _blue;
                    break;
                case Light.LightColors.GREENR:
                    ColorBox.Background = _green;
                    break;
                case Light.LightColors.REDR:
                    ColorBox.Background = _red;
                    break;
                case Light.LightColors.WHITER:
                    ColorBox.Background = _white;
                    break;
                case Light.LightColors.MAGNTAR:
                    ColorBox.Background = _magenta;
                    break;
                default:
                    ColorBox.Background = _white;
                    break;
            }        }

    }
}
using IntelliCenterControl.Models;
using System;
using Rg.Plugins.Popup.Extensions;
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

        private async void LightGroupSettingsButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                // Open a PopupPage
                await Navigation.PushPopupAsync(new LightGroupSettingsPage(b.BindingContext), true);
            }
        }
    }



}
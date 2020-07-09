using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliCenterControl.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PumpStatus : ContentView
    {
        public PumpStatus()
        {
            InitializeComponent();

            ActiveToggle.PropertyChanged += ActiveToggle_PropertyChanged;
            UpdateVisibility();
        }

        private void ActiveToggle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsToggled")
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            if (ActiveToggle.IsToggled)
            {
                PumpImage.Source = ImageSource.FromFile("pump_on.png");
            }
            else
            {
                PumpImage.Source = ImageSource.FromFile("pump_off.png");
            }
        }
    }
}
﻿using IntelliCenterControl.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BodyStatus : ContentView
    {
        public BodyStatus()
        {
            InitializeComponent();

            HeatMode.PropertyChanged += HeatMode_PropertyChanged;
        }

        private void HeatMode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (Enum.TryParse<Body.HeatModes>(HeatMode.Text, out var hmEnum))
            {
                switch (hmEnum)
                {
                    case Body.HeatModes.Off:
                        HeatImage.Source = null;
                        break;
                    case Body.HeatModes.Flame:
                    case Body.HeatModes.Hybrid:
                    case Body.HeatModes.MasterTemp:
                    case Body.HeatModes.MaxETemp:
                    case Body.HeatModes.Ultra:
                        HeatImage.Source = ImageSource.FromFile("flame.png");
                        break;
                    case Body.HeatModes.Solar:
                        HeatImage.Source = ImageSource.FromFile("solar.png");
                        break;
                    case Body.HeatModes.Flake:
                        HeatImage.Source = ImageSource.FromFile("flake.png");
                        break;

                }
            }
        }

        private void TempEntry_Completed(object sender, EventArgs e)
        {
            if (sender is Entry temp)
            {
                var model = (Body)temp.BindingContext;

                model?.SendTemperatureCommand.Execute(null);
            }
        }
    }
}
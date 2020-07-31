using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IntelliCenterControl.Views
{
    public class LightColors
    {
        private static LightColors instance;
        private static readonly object mutex = new object();

        
        public static LightColors Instance
        {
            get
            {
                lock (mutex)
                {

                    instance ??= new LightColors();
                }

                return instance;
            }
        }

        public LinearGradientBrush Party { get; } = new LinearGradientBrush();

        public LinearGradientBrush American { get; } = new LinearGradientBrush();

        public LinearGradientBrush Caribbean { get; } = new LinearGradientBrush();

        public LinearGradientBrush Sunset { get; } = new LinearGradientBrush();

        public LinearGradientBrush Romance { get; } = new LinearGradientBrush();

        public LinearGradientBrush Royal { get; } = new LinearGradientBrush();

        public SolidColorBrush Magenta { get; } = new SolidColorBrush(Color.FromHex("EC008B"));

        public SolidColorBrush Red { get; } = new SolidColorBrush(Color.Red);

        public SolidColorBrush Green { get; } = new SolidColorBrush(Color.Green);

        public SolidColorBrush Blue { get; } = new SolidColorBrush(Color.Blue);

        public SolidColorBrush White { get; } = new SolidColorBrush(Color.White);

        public LinearGradientBrush Sam { get; } = new LinearGradientBrush();

        private LightColors()
        {
            Party.GradientStops.Add(new GradientStop(Color.FromHex("E7322F"), 0f));
            Party.GradientStops.Add(new GradientStop(Color.FromHex("ea2132"), 0.2f));
            Party.GradientStops.Add(new GradientStop(Color.FromHex("cb5ea7"), 0.4f));
            Party.GradientStops.Add(new GradientStop(Color.FromHex("59449c"), 0.6f));
            Party.GradientStops.Add(new GradientStop(Color.FromHex("4cb85b"), 0.8f));
            Party.GradientStops.Add(new GradientStop(Color.FromHex("f7f597"), 1f));
           

            American.GradientStops.Add(new GradientStop(Color.FromHex("260d2b"), 0f));
            American.GradientStops.Add(new GradientStop(Color.FromHex("006399"), 0.2f));
            American.GradientStops.Add(new GradientStop(Color.FromHex("D52E5F"), 0.4f));
            American.GradientStops.Add(new GradientStop(Color.FromHex("260d2b"), 0.6f));
            American.GradientStops.Add(new GradientStop(Color.FromHex("006399"), 0.8f));
            American.GradientStops.Add(new GradientStop(Color.FromHex("D52E5F"), 1f));
            

            Caribbean.GradientStops.Add(new GradientStop(Color.FromHex("184937"), 0f));
            Caribbean.GradientStops.Add(new GradientStop(Color.FromHex("58c872"), 0.2f));
            Caribbean.GradientStops.Add(new GradientStop(Color.FromHex("2b398a"), 0.4f));
            Caribbean.GradientStops.Add(new GradientStop(Color.FromHex("cd62b3"), 0.6f));
            Caribbean.GradientStops.Add(new GradientStop(Color.FromHex("58c872"), 0.8f));
            Caribbean.GradientStops.Add(new GradientStop(Color.FromHex("f3f6a1"), 1f));
           

            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("FDBA56"), 0f));
            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("D3535C"), 0.15f));
            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("683B93"), 0.3f));
            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("EA832A"), 0.45f));
            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("7C73B4"), 0.60f));
            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("F6644E"), 0.75f));
            Sunset.GradientStops.Add(new GradientStop(Color.FromHex("FDBF63"), 1f));
           

            Romance.GradientStops.Add(new GradientStop(Color.FromHex("87d5e1"), 0f));
            Romance.GradientStops.Add(new GradientStop(Color.FromHex("6d79c3"), 0.5f));
            Romance.GradientStops.Add(new GradientStop(Color.FromHex("aa67b5"), 1f));
           

            Royal.GradientStops.Add(new GradientStop(Color.FromHex("FFF165"), 0f));
            Royal.GradientStops.Add(new GradientStop(Color.FromHex("1DA453"), 0.5f));
            Royal.GradientStops.Add(new GradientStop(Color.FromHex("2E3192"), 1f));
            

            Sam.GradientStops.Add(new GradientStop(Color.Red, 0f));
            Sam.GradientStops.Add(new GradientStop(Color.Green, 0.25f));
            Sam.GradientStops.Add(new GradientStop(Color.Blue, 0.5f));
            Sam.GradientStops.Add(new GradientStop(Color.White, 0.75f));
            Sam.GradientStops.Add(new GradientStop(Color.FromHex("EC008B"), 1f));
            
        }
    }
}

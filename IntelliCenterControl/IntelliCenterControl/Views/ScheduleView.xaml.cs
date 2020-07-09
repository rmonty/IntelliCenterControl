using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleView : ContentView
    {
        public ScheduleView()
        {
            InitializeComponent();

            ActiveToggle.PropertyChanged += ActiveToggle_PropertyChanged;
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
            ButtonImage.Source = ImageSource.FromFile(ActiveToggle.IsToggled ? "radio_button_on_large.png" : "radio_button_off_large.png");
        }
    }
}
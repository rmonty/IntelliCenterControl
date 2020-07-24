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
            PumpImage.Source = ImageSource.FromFile(ActiveToggle.IsToggled ? "pump_on.png" : "pump_off.png");
        }
    }
}
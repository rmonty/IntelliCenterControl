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
            Device.BeginInvokeOnMainThread(
            () =>
            {
                ButtonImage.Source = ImageSource.FromFile(ActiveToggle.IsToggled
                    ? "radio_button_on_large.png"
                    : "radio_button_off_large.png");
            });

        }
    }


}
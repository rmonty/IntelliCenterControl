using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IntelliCenterControl.Services;
using IntelliCenterControl.Views;

namespace IntelliCenterControl
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            Device.SetFlags(new string[] { "CarouselView_Experimental", "RadioButton_Experimental", "SwipeView_Experimental" });
            DependencyService.Register<IntelliCenterDataInterface>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            MessagingCenter.Send<App>(this, "Starting");
        }

        protected override void OnSleep()
        {
            MessagingCenter.Send<App>(this, "Sleeping");
        }

        protected override void OnResume()
        {
            MessagingCenter.Send<App>(this, "Resume");
        }

        protected override void CleanUp()
        {
            MessagingCenter.Send<App>(this, "CleanUp");
            base.CleanUp();
        }
    }
}

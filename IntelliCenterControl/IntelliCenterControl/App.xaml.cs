using System;
using Xamarin.Forms;
using IntelliCenterControl.Services;
using IntelliCenterControl.Views;

namespace IntelliCenterControl
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
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
        
    }
}

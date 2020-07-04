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

            DependencyService.Register<IntelliCenterDataInterface>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

using Xamarin.Forms;
using IntelliCenterControl.Services;
using IntelliCenterControl.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.ViewModels;

namespace IntelliCenterControl
{
    public partial class App : Application
    {

        public App()
        {
            AppCenter.Start("94ace755-cc3f-4f64-91a7-6c7956267bf8",
                typeof(Analytics), typeof(Crashes));
            InitializeComponent();
            DependencyService.Register<IntelliCenterDataInterface>();
            SimpleIoc.Default.Register<ILogService, LogService>();
            SimpleIoc.Default.Register<ControllerViewModel>();
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

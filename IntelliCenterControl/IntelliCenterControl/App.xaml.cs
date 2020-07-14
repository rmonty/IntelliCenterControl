using Xamarin.Forms;
using IntelliCenterControl.Services;
using IntelliCenterControl.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace IntelliCenterControl
{
    public partial class App : Application
    {

        public App()
        {
            AppCenter.Start("android=94ace755-cc3f-4f64-91a7-6c7956267bf8;" ,
                typeof(Analytics), typeof(Crashes));
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

using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Models;
using IntelliCenterControl.Services;
using IntelliCenterControl.ViewModels;
using IntelliCenterControl.Views;
using Xamarin.Forms;

namespace IntelliCenterControl
{
    public partial class App : Application
    {

        public App()
        {
            Device.SetFlags(new[] { "Expander_Experimental", "RadioButton_Experimental", "SwipeView_Experimental" });

            InitializeComponent();

            SimpleIoc.Default.Register<ICloudLogService, CloudLogService>();
            SimpleIoc.Default.Register<IDataInterface<IntelliCenterConnection>, IntelliCenterDataInterface>();
            SimpleIoc.Default.Register<ILogService, LogService>();
            SimpleIoc.Default.Register<ControllerViewModel>();

            MainPage = new MainPage();

        }

        protected override void OnStart()
        {
            SimpleIoc.Default.GetInstance<ICloudLogService>().Initialize("94ace755-cc3f-4f64-91a7-6c7956267bf8");
            MessagingCenter.Send(this, "Starting");
        }

        protected override void OnSleep()
        {
            MessagingCenter.Send(this, "Sleeping");
        }

        protected override void OnResume()
        {
            MessagingCenter.Send(this, "Resume");
        }


    }
}

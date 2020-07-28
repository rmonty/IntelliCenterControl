
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Services;
using Plugin.Toasts;
using Xamarin.Forms;

namespace IntelliCenterControl.Droid
{
    [Activity(Label = "IntelliCenterControl", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            AiForms.Renderers.Droid.SettingsViewInit.Init();
            UserDialogs.Init(this);
            DependencyService.Register<ToastNotification>();
            ToastNotification.Init(this);

            LoadApplication(new App());

            this.Bootstraping();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            //MessagingCenter.Subscribe<StartMacroTaskMessage> (this, "StartMacroTaskMessage", message => {
            //    var intent = new Intent (this, typeof(MacroTaskService));
            //    StartService (intent);
            //});

            //MessagingCenter.Subscribe<StopMacroTaskMessage> (this, "StopMacroTaskMessage", message => {
            //    var intent = new Intent (this, typeof(MacroTaskService));
            //    StopService (intent);
            //});
            base.OnCreate(savedInstanceState, persistentState);
        }

        private void Bootstraping()
        {
            var assembly = this.GetType().Assembly;
            var assemblyName = assembly.GetName().Name;

            SimpleIoc.Default.GetInstance<ILogService>().Initialize(assembly, assemblyName);
        }


    }
}
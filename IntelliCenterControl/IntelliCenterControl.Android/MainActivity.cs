using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Services;
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
            //DependencyService.Register<ToastNotification>();
            //ToastNotification.Init(this);
            LoadApplication(new App());
            
            this.Bootstraping();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void Bootstraping()
        {
            var assembly = this.GetType().Assembly;
            var assemblyName = assembly.GetName().Name;

            SimpleIoc.Default.GetInstance<ILogService>().Initialize(assembly, assemblyName);
        }
    

    }
}
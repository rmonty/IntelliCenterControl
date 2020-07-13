using IntelliCenterControl.Models;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using IntelliCenterControl.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IntelliCenterControl.ViewModels
{
    public class AboutViewModel : BaseViewModel<object>
    {
        public string AppName => AppInfo.Name;

        public string PackageName => AppInfo.PackageName;

        private string _version;

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }


        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://github.com/rmonty/IntelliCenterControl"));

            var customAttributes = (object[])typeof(AboutPage)
                .GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute));
            if (customAttributes.Length > 0)
            {
                Version = ((AssemblyInformationalVersionAttribute)customAttributes[0]).InformationalVersion;
            }
        }

        public ICommand OpenWebCommand { get; }
    }
}
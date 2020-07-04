using IntelliCenterControl.Models;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IntelliCenterControl.ViewModels
{
    public class AboutViewModel : BaseViewModel<object>
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://troublefreepool.com"));
        }

        public ICommand OpenWebCommand { get; }
    }
}
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Xamarin.Forms;
using IntelliCenterControl.Models;
using IntelliCenterControl.ViewModels;
using Plugin.Toast;
using Xamarin.Essentials;

namespace IntelliCenterControl.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ControllerMainView : ContentPage
    {
        ControllerViewModel viewModel;

        public ControllerMainView()
        {
            InitializeComponent();

            BindingContext = viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();

            viewModel.DataInterface.ConnectionChanged += DataInterface_ConnectionChanged;

            MessagingCenter.Subscribe<App>(this, "Starting", (sender) =>
            {
                viewModel.LoadHardwareDefinitionCommand.Execute(true);
            });
            MessagingCenter.Subscribe<App>(this, "Sleeping", (sender) =>
            {
                viewModel.ClosingCommand.Execute(null);
            });
            MessagingCenter.Subscribe<App>(this, "Resume", (sender) =>
            {
                viewModel.LoadHardwareDefinitionCommand.Execute(true);
            });


            Task.Run(async() => await CheckAndRequestStoragePermission());

        }

        private void DataInterface_ConnectionChanged(object sender, IntelliCenterConnection e)
        {
            CrossToastPopUp.Current.ShowToastMessage(e.State.ToString() + "...");
        }

        private async void UpdateIP_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("Server URL", "Please Enter Server URL", initialValue : Settings.ServerURL);

            //var validUrl = Uri.TryCreate(result, UriKind.Absolute, out var uriResult);
                           // && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!String.IsNullOrEmpty(result))
            {
                Settings.ServerURL = result;
                viewModel.UpdateIpAddress();
            }
            
        }

        private void RefreshConnection_Clicked(object sender, EventArgs e)
        {
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();

        //    if (String.IsNullOrEmpty(viewModel.HardwareDefinition.messageId.ToString()))
        //        viewModel.IsBusy = true;

        //}

        public async Task<PermissionStatus> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            

            if (status != PermissionStatus.Granted && !Settings.StorageAccessAsked)
            {
                var result = await DisplayAlert("Storage Access",
                    @"This application uses storage for debug logging. These logs are not sent from your device and are only used for debugging purposes if you encounter a problem. Restricting access will not affect app usage.",
                    "Yes", "No");
                Settings.StorageAccessAsked = true;
                
                if (result)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                }
            }
            
            return status;
        }




    }
}
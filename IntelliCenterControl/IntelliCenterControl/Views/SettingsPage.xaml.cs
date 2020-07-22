using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Models;
using IntelliCenterControl.ViewModels;
using Plugin.Toast;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        ControllerViewModel viewModel;
        private Settings _settings = Settings.Instance;
        public SettingsPage()
        {
            InitializeComponent();
            viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();
            viewModel.DataInterface.ConnectionChanged += DataInterface_ConnectionChanged;
        }

        private void DataInterface_ConnectionChanged(object sender, IntelliCenterConnection e)
        {
            CrossToastPopUp.Current.ShowToastMessage(e.State.ToString() + "...");

            ConnectedIcon.IconImageSource = e.State switch
            {
                IntelliCenterConnection.ConnectionState.Disconnected => ImageSource.FromFile("not_connected.png"),
                IntelliCenterConnection.ConnectionState.Connected => ImageSource.FromFile("connected.png"),
                IntelliCenterConnection.ConnectionState.Connecting => ImageSource.FromFile("not_connected.png"),
                IntelliCenterConnection.ConnectionState.Reconnecting => ImageSource.FromFile("not_connected.png"),
                _ => ImageSource.FromFile("not_connected.png")
            };
        }

        private async void UpdateIP_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayActionSheet("Server URL", "Cancel", null, "Home URL", "Away URL");

            if (!string.IsNullOrEmpty(result))
            {
                switch (result)
                {
                    case "Home URL":
                        _settings.ServerURL = _settings.HomeURL;
                        viewModel.UpdateIpAddress();
                        break;
                    case "Away URL":
                        _settings.ServerURL = _settings.AwayURL;
                        viewModel.UpdateIpAddress();
                        break;
                }

            }
        }

        private void RefreshConnection_Clicked(object sender, EventArgs e)
        {
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
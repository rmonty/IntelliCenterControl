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
    public partial class ChemPage : ContentPage
    {
        ControllerViewModel viewModel;
        public ChemPage()
        {
            InitializeComponent();

            BindingContext = viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.DataInterface.ConnectionChanged += DataInterface_ConnectionChanged;
        }

        private void DataInterface_ConnectionChanged(object sender, Models.IntelliCenterConnection e)
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

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "StatusMessage":
                    Device.BeginInvokeOnMainThread(
                        () => { CrossToastPopUp.Current.ShowToastMessage(viewModel.StatusMessage); });
                    break;
            }
        }

        private void RefreshConnection_Clicked(object sender, EventArgs e)
        {
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }
    }
}
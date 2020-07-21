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
            switch (e.State)
            {
                case IntelliCenterConnection.ConnectionState.Disconnected:
                    ConnectedIcon.IconImageSource = ImageSource.FromFile("not_connected.png");
                    break;
                case IntelliCenterConnection.ConnectionState.Connected:
                    ConnectedIcon.IconImageSource = ImageSource.FromFile("connected.png");
                    break;
                case IntelliCenterConnection.ConnectionState.Connecting:
                    ConnectedIcon.IconImageSource = ImageSource.FromFile("not_connected.png");
                    break;
                case IntelliCenterConnection.ConnectionState.Reconnecting:
                    ConnectedIcon.IconImageSource = ImageSource.FromFile("not_connected.png");
                    break;
                default:
                    ConnectedIcon.IconImageSource = ImageSource.FromFile("not_connected.png");
                    break;
            }
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

        //private void RefreshConnection_Clicked(object sender, EventArgs e)
        //{
        //    viewModel.LoadHardwareDefinitionCommand.Execute(true);
        //}
    }
}
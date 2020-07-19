
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Models;
using IntelliCenterControl.ViewModels;
using Plugin.Toast;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleEditPage : ContentPage
    {
        ControllerViewModel viewModel;
        
        public ScheduleEditPage()
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

        private void RefreshConnection_Clicked(object sender, System.EventArgs e)
        {
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }

        private void Save_Clicked(object sender, System.EventArgs e)
        {
            if (((Button) sender).BindingContext is Schedule context)
            {
                ((Button)sender).IsEnabled = false;
                context.Expanded = false;
                context.SaveScheduleCommand.Execute(null);
            }
        }

        private async void Delete_Clicked(object sender, System.EventArgs e)
        {
            if (((Button)sender).BindingContext is Schedule context)
            {
                context.Expanded = false;
                if (context.Hname == null)
                {
                    viewModel.Schedules.Remove(context);
                    Device.BeginInvokeOnMainThread(
                        () => { CrossToastPopUp.Current.ShowToastMessage("Item Deleted"); });
                }
                else
                {
                    var result = await DisplayAlert("Delete Item",
                        @"Are you sure you want to delete scheduled item.",
                        "Yes", "No");
                    if (result)
                    {
                        ((Button)sender).IsEnabled = false;
                        context.DeleteScheduleCommand.Execute(null);
                    }
                }
            }
        }
    }
}
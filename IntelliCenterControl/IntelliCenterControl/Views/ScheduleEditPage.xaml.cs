
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

        private void RefreshConnection_Clicked(object sender, System.EventArgs e)
        {
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }

        private void Save_Clicked(object sender, System.EventArgs e)
        {
            if (((SwipeItem) sender).BindingContext is Schedule context)
            {
                //((Button)sender).IsEnabled = false;
                context.Expanded = false;
                context.SaveScheduleCommand.Execute(null);
            }
        }

        private async void Delete_Clicked(object sender, System.EventArgs e)
        {
            if (((SwipeItem)sender).BindingContext is Schedule context)
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
                        //((Button)sender).IsEnabled = false;
                        context.DeleteScheduleCommand.Execute(null);
                    }
                }
            }
        }

        
    }
}
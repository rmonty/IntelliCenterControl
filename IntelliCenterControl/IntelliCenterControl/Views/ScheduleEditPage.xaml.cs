using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Models;
using IntelliCenterControl.ViewModels;
using Plugin.Toast;
using Xamarin.Essentials;
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
                    MainThread.BeginInvokeOnMainThread(
                        () => { CrossToastPopUp.Current.ShowToastSuccess("Item Deleted"); });
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
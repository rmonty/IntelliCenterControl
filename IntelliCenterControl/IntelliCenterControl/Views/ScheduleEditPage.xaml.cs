using Acr.UserDialogs;
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Models;
using IntelliCenterControl.ViewModels;
using Plugin.Toasts;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IntelliCenterControl.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleEditPage : ContentPage
    {
        ControllerViewModel viewModel;
        IToastNotificator _notificator;

        public ScheduleEditPage()
        {
            InitializeComponent();
            _notificator = DependencyService.Get<IToastNotificator>();
            BindingContext = viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();
        }


        private void Save_Clicked(object sender, System.EventArgs e)
        {
            if (((SwipeItem)sender).BindingContext is Schedule context)
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
                        () => {
                            ToastConfig.DefaultBackgroundColor = Color.FromHex("#2196F3");
                            ToastConfig.DefaultMessageTextColor = Color.White;
                            UserDialogs.Instance.Toast(new ToastConfig("Item Deleted").SetDuration(1000)
                                .SetPosition(ToastPosition.Bottom));
                            });
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
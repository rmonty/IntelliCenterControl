﻿using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.Models;
using IntelliCenterControl.ViewModels;

using System.ComponentModel;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using Plugin.Toasts;

namespace IntelliCenterControl.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        ControllerViewModel viewModel;
        private Page cPage;
        Settings _settings = Settings.Instance;
        IToastNotificator _notificator;

        public MainPage()
        {
            InitializeComponent();
            cPage = new NavigationPage(new ChemPage())
            {
                Title = "Chem",
                IconImageSource = "chem_icon.png"
            };
            _notificator = DependencyService.Get<IToastNotificator>();
            BindingContext = viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();
            
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
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


            //Task.Run(async () => await CheckAndRequestStoragePermission());

        }

        private void DataInterface_ConnectionChanged(object sender, IntelliCenterConnection e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConnectedIcon.IconImageSource = e.State switch
                {
                    IntelliCenterConnection.ConnectionState.Disconnected => ImageSource.FromFile("not_connected.png"),
                    IntelliCenterConnection.ConnectionState.Connected => ImageSource.FromFile("connected.png"),
                    IntelliCenterConnection.ConnectionState.Connecting => ImageSource.FromFile("not_connected.png"),
                    IntelliCenterConnection.ConnectionState.Reconnecting => ImageSource.FromFile("not_connected.png"),
                    _ => ImageSource.FromFile("not_connected.png")
                };

                if (e.State == IntelliCenterConnection.ConnectionState.Disconnected)
                {
                    ToastConfig.DefaultBackgroundColor = Color.Yellow;
                    ToastConfig.DefaultMessageTextColor = Color.Black;
                    UserDialogs.Instance.Toast(new ToastConfig(e.State.ToString()).SetDuration(1000)
                        .SetPosition(ToastPosition.Bottom));
                }
                else
                {
                    ToastConfig.DefaultBackgroundColor = Color.Green;
                    ToastConfig.DefaultMessageTextColor = Color.White;
                    UserDialogs.Instance.Toast(new ToastConfig(e.State.ToString()).SetDuration(1000)
                        .SetPosition(ToastPosition.Bottom));
                }

            });
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (e.PropertyName)
                {
                    case "ChemInstalled":
                        if (viewModel.ChemInstalled)
                        {
                            MainAppPage.Children.Insert(1, cPage);
                        }
                        else
                        {
                            if (MainAppPage.Children.Contains(cPage))
                            {
                                MainAppPage.Children.Remove(cPage);

                            }
                        }
                        break;
                    case "StatusMessage":
                        if (viewModel.StatusMessage == "Unauthorized")
                        {
                            ToastConfig.DefaultBackgroundColor = Color.Red;
                            ToastConfig.DefaultMessageTextColor = Color.White;
                            UserDialogs.Instance.Toast(new ToastConfig("Unauthorized").SetDuration(1000)
                                .SetPosition(ToastPosition.Bottom));
                        }
                        else
                        {
                            ToastConfig.DefaultBackgroundColor = Color.FromHex("#2196F3");
                            ToastConfig.DefaultMessageTextColor = Color.White;
                            UserDialogs.Instance.Toast(new ToastConfig(viewModel.StatusMessage).SetDuration(1000)
                                .SetPosition(ToastPosition.Bottom));
                        }
                        break;

                }
            });
        }

        private void RefreshConnection_Clicked(object sender, System.EventArgs e)
        {
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }

        private async void UpdateIP_Clicked(object sender, System.EventArgs e)
        {
            string result;
            if (_settings.ServerURL == _settings.HomeURL)
            {
                result = await DisplayActionSheet("Server URL", "Cancel", null, "Home URL (Active)", "Away URL");
            }
            else if (_settings.ServerURL == _settings.AwayURL)
            {
                result = await DisplayActionSheet("Server URL", "Cancel", null, "Home URL", "Away URL (Active)");
            }
            else
            {
                result = await DisplayActionSheet("Server URL", "Cancel", null, "Home URL", "Away URL");
            }


            if (!string.IsNullOrEmpty(result))
            {
                switch (result)
                {
                    case "Home URL":
                        _settings.ServerURL = _settings.HomeURL;
                        viewModel.UpdateIpAddressAsync();
                        break;
                    case "Away URL":
                        _settings.ServerURL = _settings.AwayURL;
                        viewModel.UpdateIpAddressAsync();
                        break;
                }

            }
        }

        public async Task<PermissionStatus> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status != PermissionStatus.Granted)
            {
                if (status != PermissionStatus.Denied) status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                if (status != PermissionStatus.Granted && !_settings.StorageAccessAsked)
                {
                    await DisplayAlert("Storage Access",
                        @"This application can us storage for debug logging. These logs are not sent from your device and are only used for debugging purposes if you encounter a problem. Restricting access will not affect app usage.  To enable storage you can turn on manually in phone settings",
                        "Ok");
                    _settings.StorageAccessAsked = true;
                }

            }

            return status;
        }
    }
}
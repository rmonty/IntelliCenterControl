﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using IntelliCenterControl.Models;
using IntelliCenterControl.Views;
using IntelliCenterControl.ViewModels;

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

            BindingContext = viewModel = new ControllerViewModel();
            

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
                viewModel.SubscribeDataCommand.Execute(null);
            });
            MessagingCenter.Subscribe<App>(this, "CleanUp", (sender) =>
            {
                viewModel.ClosingCommand.Execute(null);
            });
        }

        private async void ConnectToolBarItem_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("Server URL", "Please Enter Server URL", initialValue : Settings.ServerURL);

            bool validUrl = Uri.TryCreate(result, UriKind.Absolute, out var uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (validUrl)
            {
                Settings.ServerURL = result;
                viewModel.UpdateIPAddress();
            }
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();

        //    if (String.IsNullOrEmpty(viewModel.HardwareDefinition.messageId.ToString()))
        //        viewModel.IsBusy = true;

        //}





    }
}
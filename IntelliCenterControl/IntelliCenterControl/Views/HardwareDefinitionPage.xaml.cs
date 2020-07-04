using System;
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
    public partial class HardwareDefinitionPage : ContentPage
    {
        HardwareDefinitionViewModel viewModel;

        public HardwareDefinitionPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new HardwareDefinitionViewModel();
            viewModel.LoadHardwareDefinitionCommand.Execute(true);
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (String.IsNullOrEmpty(viewModel.HardwareDefinition.messageId.ToString()))
                viewModel.IsBusy = true;
                        
        }

        
    
    }
}
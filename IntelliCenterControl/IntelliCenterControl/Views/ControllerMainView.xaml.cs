using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.ViewModels;
using System.ComponentModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IntelliCenterControl.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer

    public partial class ControllerMainView : ContentPage
    {
        ControllerViewModel viewModel;

        public ControllerMainView()
        {
            InitializeComponent();

            BindingContext = viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();

            viewModel.PropertyChanged += ViewModel_PropertyChanged;

        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(
                          () =>
                          {
                              switch (e.PropertyName)
                              {
                                  case "HasCircuits":
                                      if (viewModel.HasCircuits)
                                      {
                                          Row3.Height = new GridLength(1, GridUnitType.Star);
                                          Grid.SetRow(LightFrame, 3);
                                          Grid.SetRow(LightImage, 3);
                                          Grid.SetRow(LightView, 3);
                                          Grid.SetRowSpan(LightFrame, 1);
                                          Grid.SetRowSpan(LightImage, 1);
                                          Grid.SetRowSpan(LightView, 1);
                                      }
                                      else
                                      {
                                          Row3.Height = new GridLength(0);
                                          Grid.SetRow(LightFrame, 2);
                                          Grid.SetRow(LightImage, 2);
                                          Grid.SetRow(LightView, 2);
                                          Grid.SetRowSpan(LightFrame, 2);
                                          Grid.SetRowSpan(LightImage, 2);
                                          Grid.SetRowSpan(LightView, 2);
                                      }
                                      break;
                                  case "HasCircuitGroups":
                                      if (viewModel.HasCircuits)
                                      {
                                          Row2.Height = new GridLength(1.25, GridUnitType.Star);
                                          Grid.SetRowSpan(TempFrame, 1);
                                          Grid.SetRowSpan(TempGrid, 1);
                                          Grid.SetRowSpan(BodyFrame, 1);
                                          Grid.SetRowSpan(BodyImage, 1);
                                          Grid.SetRowSpan(BodyGrid, 1);
                                      }
                                      else
                                      {
                                          Row2.Height = new GridLength(0);
                                          Grid.SetRowSpan(TempFrame, 2);
                                          Grid.SetRowSpan(TempGrid, 2);
                                          Grid.SetRowSpan(BodyFrame, 2);
                                          Grid.SetRowSpan(BodyImage, 2);
                                          Grid.SetRowSpan(BodyGrid, 2);
                                      }
                                      break;
                              }
                          });
        }




    }
}
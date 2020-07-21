using System.ComponentModel;
using GalaSoft.MvvmLight.Ioc;
using IntelliCenterControl.ViewModels;
using Xamarin.Forms;

namespace IntelliCenterControl.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        ControllerViewModel viewModel;
        private Page cPage;
        private Page aboutPage;

        public MainPage()
        {
            InitializeComponent();
            cPage = new NavigationPage(new ChemPage())
            {
                Title="Chem"
            };

            aboutPage = MainAppPage.Children[^1];

            BindingContext = viewModel = SimpleIoc.Default.GetInstance<ControllerViewModel>();

            viewModel.PropertyChanged += ViewModel_PropertyChanged; ;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ChemInstalled":
                    if (viewModel.ChemInstalled)
                    {
                        MainAppPage.Children.Insert(2, cPage);
                        

                    }
                    else
                    {
                        if (MainAppPage.Children.Contains(cPage))
                        {
                            MainAppPage.Children.Remove(cPage);
                            
                        }
                    }
                    break;
            }
        }
    }
}